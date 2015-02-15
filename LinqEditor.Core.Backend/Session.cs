using LinqEditor.Core.CodeAnalysis.Compiler;
using LinqEditor.Core.CodeAnalysis.Services;
using LinqEditor.Core.Containers;
using LinqEditor.Core.Helpers;
using LinqEditor.Core.Models.Analysis;
using LinqEditor.Core.Models.Editor;
using LinqEditor.Core.Schema.Services;
using LinqEditor.Core.Settings;
using LinqEditor.Core.Templates;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace LinqEditor.Core.Backend
{
    /// <summary>
    /// well designed class with single responsibility of doing everything
    /// </summary>
    public class Session : ISession
    {
        // session is one-time bind only
        private bool _codeSession;
        private bool _initialized = false;
        private bool _canReinitialize = false;
        private bool _loadedAppDomain = false;
        private Guid _id;

        int _prevHash;
        string _prevSrc;

        private ISqlSchemaProvider _schemaProvider;
        private ITemplateService _generator;

        private Stopwatch _watch;
        private IIsolatedCodeContainerFactory _codeContainerFactory;
        private IIsolatedDatabaseContainerFactory _databaseContainerFactory;
        private IConnectionStore _connectionStore;
        private ITemplateCodeAnalysis _codeAnalysis;
        private Connection _connection;

        private IIsolatedCodeContainer _codeContainer;
        private IIsolatedDatabaseContainer _databaseContainer;

        public Guid Id { get { return _id; } }

        public Session(Guid contextId, ISqlSchemaProvider schemaProvider, ITemplateService generator,// ISchemaStore userSettings,
            IIsolatedCodeContainerFactory codeContainerFactory, IIsolatedDatabaseContainerFactory databaseContainerFactory,
            IConnectionStore connectionStore, ITemplateCodeAnalysis codeAnalysis)
        {
            _id = contextId; // id passd to the factory
            _schemaProvider = schemaProvider;
            _generator = generator;
            _watch = new Stopwatch();
            _codeContainerFactory = codeContainerFactory;
            _databaseContainerFactory = databaseContainerFactory;
            _connectionStore = connectionStore;
            _codeAnalysis = codeAnalysis;
        }

        public InitializeResult Initialize(Guid connectionId)
        {
            if (_initialized) throw new InvalidOperationException("Cannot initialize more then once");
            _codeSession = connectionId == _connectionStore.CodeConnection.Id;
            _initialized = true;
            return _codeSession ? InitCode() : InitDatabase(connectionId);
        }

        public InitializeResult Reinitialize()
        {
            if (!_canReinitialize) throw new InvalidOperationException("Cannot reinitialize unless execute was cancelled");
            _canReinitialize = false;

            _loadedAppDomain = false;
            LoadAppDomain();

            return _codeSession ? InitCode() : InitDatabase(_connection.Id);
        }

        public ExecuteResult Execute(string sourceFragment)
        {
            return Execute(sourceFragment, CancellationToken.None);
        }

        public ExecuteResult Execute(string sourceFragment, CancellationToken ct)
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
                // this is probably wrong
                ct.Register(() => 
                {
                    _canReinitialize = true;
                    if (_codeSession)
                    {
                        _codeContainer.Dispose();
                        _codeContainerFactory.Release(_codeContainer);
                    }
                    else
                    {
                        _databaseContainer.Dispose();
                        _databaseContainerFactory.Release(_databaseContainer);
                    }
                });

                ExecuteResult containerResult = null;

                try
                {
                    // if the src differs, the bytes must differ
                    if (_prevHash != 0 && _prevSrc != null && _prevSrc != sourceFragment)
                    {
                        Debug.Assert(_prevHash != result.AssemblyBytes.GetHashCode());
                    }

                    _prevHash = result.AssemblyBytes.GetHashCode();
                    _prevSrc = sourceFragment;

                    containerResult = _codeSession ? _codeContainer.Value.Execute(result.AssemblyBytes) :
                    _databaseContainer.Value.Execute(result.AssemblyBytes);
                }
                catch (AppDomainUnloadedException)
                {
                    return new ExecuteResult
                    {
                        Success = false,
                        Kind = _codeSession ? ProgramType.Code : ProgramType.Database,
                        QueryText = "Cancelled",
                        Duration = _watch.ElapsedMilliseconds,
                        CodeOutput = "Cancelled"
                    };
                }

                return new ExecuteResult
                {
                    Success = containerResult.Success,
                    QueryText = containerResult.QueryText,
                    Tables = containerResult.Tables,
                    Warnings = result.Warnings,
                    Duration = _watch.ElapsedMilliseconds,
                    CodeOutput = containerResult.CodeOutput,
                    Kind = _codeSession ? ProgramType.Code : ProgramType.Database,
                };
            }

            return new ExecuteResult
            {
                Success = false,
                Errors = result.Errors,
                Warnings = result.Warnings,
                Duration = _watch.ElapsedMilliseconds,
                Kind = _codeSession ? ProgramType.Code : ProgramType.Database,
            };
        }

        public LoadAppDomainResult LoadAppDomain()
        {
            if (_loadedAppDomain) throw new InvalidOperationException("Cannot load AppDomain more then once, unless execute was cancelled");
            Exception exn = null;
            // loads schema in new appdomain
            if (_codeSession)
            {
                _codeContainer = _codeContainerFactory.Create();
                var res = _codeContainer.Value.Initialize();
                exn = res.Error;
            }
            else
            {
                _databaseContainer = _databaseContainerFactory.Create();
                var res = _databaseContainer.Value.Initialize(_connection.CachedSchemaFileName);
                exn = res.Error;
            }

            _loadedAppDomain = true;

            return new LoadAppDomainResult
            {
                Error = exn, // only member set in runner
            };
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
            _connection = _connectionStore.Connections.Where(x => x.Id == id).SingleOrDefault();
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
