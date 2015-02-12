using LinqEditor.Core.Models.Editor;
using LinqEditor.Core.Settings;
using LinqEditor.Core.CodeAnalysis.Compiler;
using LinqEditor.Core.Context;
using LinqEditor.Core.Helpers;
using LinqEditor.Core.Schema.Services;
using LinqEditor.Core.Templates;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using LinqEditor.Core.Containers;
using LinqEditor.Core.CodeAnalysis.Services;
using LinqEditor.Core.Models.Analysis;

namespace LinqEditor.Core.Backend
{
    /// <summary>
    /// well designed class with single responsibility of doing everything
    /// </summary>
    public class Session : ISession, IDisposable
    {
        // session is one-time bind only
        private bool _codeSession;
        private bool _initialized = false;
        private bool _loadedAppDomain = false;

        private ISqlSchemaProvider _schemaProvider;
        private ITemplateService _generator;

        private Stopwatch _watch;
        private IIsolatedCodeContainerFactory _codeContainerFactory;
        private IIsolatedDatabaseContainerFactory _databaseContainerFactory;
        private IContainerMapper _containerMapper;
        private IConnectionStore _connectionStore;
        private ITemplateCodeAnalysis _codeAnalysis;
        private Connection _connection;

        private IIsolatedCodeContainer _codeContainer;
        private IIsolatedDatabaseContainer _databaseContainer;

        public Session(ISqlSchemaProvider schemaProvider, ITemplateService generator,// ISchemaStore userSettings,
            IIsolatedCodeContainerFactory codeContainerFactory, IIsolatedDatabaseContainerFactory databaseContainerFactory,
            IContainerMapper containerMapper, IConnectionStore connectionStore, ITemplateCodeAnalysis codeAnalysis)
        {
            _schemaProvider = schemaProvider;
            _generator = generator;
            _watch = new Stopwatch();
            _codeContainerFactory = codeContainerFactory;
            _databaseContainerFactory = databaseContainerFactory;
            _containerMapper = containerMapper;
            _connectionStore = connectionStore;
            _codeAnalysis = codeAnalysis;
        }

        public InitializeResult Initialize(Guid connectionId)
        {
            if (_initialized) throw new InvalidOperationException("Cannot initialize more then once");
            _codeSession = connectionId == Guid.Empty;
            _initialized = true;
            return _codeSession ? InitCode() : InitDatabase(connectionId);
        }

        public ExecuteResult Execute(string sourceFragment)
        {
            _watch.Restart();
            var programId = Guid.NewGuid();
            var programSource = _codeSession ? _generator.GenerateCodeStatements(programId, sourceFragment) :
                                               _generator.GenerateQuery(programId, sourceFragment, _connection.CachedSchemaNamespace );
            var references = _codeSession ? new string[] { } : new string[] { _connection.CachedSchemaFileName };
            var prefix = _codeSession ? SchemaConstants.CodePrefix : SchemaConstants.QueryPrefix;
            var result = CSharpCompiler.CompileToBytes(programSource, programId.ToIdentifierWithPrefix(prefix), references);

            if (result.Success)
            {
                var containerResult = _codeSession ? _codeContainer.Value.Execute(result.AssemblyBytes) :
                    _databaseContainer.Value.Execute(result.AssemblyBytes);

                _watch.Stop();

                return new ExecuteResult
                {
                    Success = containerResult.Success,
                    QueryText = containerResult.QueryText,
                    Tables = containerResult.Tables,
                    Warnings = result.Warnings,
                    Duration = _watch.ElapsedMilliseconds,
                    CodeOutput = containerResult.CodeOutput
                };
            }

            return new ExecuteResult
            {
                Success = false,
                Errors = result.Errors,
                Warnings = result.Warnings
            };
        }

        public LoadAppDomainResult LoadAppDomain()
        {
            if (_loadedAppDomain) throw new InvalidOperationException("Cannot load AppDomain more then once");
            Exception exn = null;
            // loads schema in new appdomain
            if (_codeSession)
            {
                _codeContainer = _codeContainerFactory.Create(_containerMapper.CodeContainer);
                var res = _codeContainer.Value.Initialize();
                exn = res.Error;
            }
            else
            {
                _databaseContainer = _databaseContainerFactory.Create(_containerMapper.MapConnectionString(_connection.ConnectionString));
                var res = _databaseContainer.Value.Initialize(_connection.CachedSchemaFileName);
                exn = res.Error;
            }

            _loadedAppDomain = true;

            return new LoadAppDomainResult
            {
                Error = exn, // only member set in runner
            };
        }

        public void Dispose()
        {
            if (_codeContainer != null && _codeContainerFactory != null)
            {
                _codeContainerFactory.Release(_codeContainer);
            }

            if (_databaseContainer != null && _databaseContainerFactory != null)
            {
                _databaseContainerFactory.Release(_databaseContainer);
            }
        }

        // expose code analysis
        public AnalysisResult Analyze(string sourceFragment, int updateIndex)
        {
            if (!_codeAnalysis.IsReady)
            {
                return new AnalysisResult { Context = UserContext.Unknown };
            }
            return _codeAnalysis.Analyze(sourceFragment, updateIndex);
        }

        public AnalysisResult Analyze(string sourceFragment)
        {
            if (!_codeAnalysis.IsReady)
            {
                return new AnalysisResult { Context = UserContext.Unknown };
            }
            return _codeAnalysis.Analyze(sourceFragment);
        }

        private InitializeResult InitCode()
        {
            _codeAnalysis.Initialize();
            return new InitializeResult();
        }

        private InitializeResult InitDatabase(Guid id)
        {
            _connection = _connectionStore.Connections.Where(x => x.Id == id).Single();
            if (_connection == null) throw new ArgumentException("unknown connection id");

            // check cache
            if (string.IsNullOrEmpty(_connection.CachedSchemaFileName))
            {
                var schemaId = Guid.NewGuid();
                var schemaNamespace = schemaId.ToIdentifierWithPrefix(SchemaConstants.SchemaPrefix);
                var sqlSchema = _schemaProvider.GetSchema(_connection.ConnectionString);
                var schemaSource = _generator.GenerateSchema(schemaId, sqlSchema);
                var result = CSharpCompiler.CompileToFile(schemaSource, schemaNamespace, Core.PathUtility.CachePath);
                
                if (result.Success)
                {
                    _connection.CachedSchemaFileName = result.AssemblyPath;
                    _connection.CachedSchemaNamespace = schemaNamespace;
                    _connectionStore.Update(_connection);
                }
            }

            _codeAnalysis.Initialize(_connection.CachedSchemaFileName);

            return new InitializeResult
            {
                AssemblyPath = _connection.CachedSchemaFileName,
                SchemaNamespace = _connection.CachedSchemaNamespace
            };
        }

    }
}
