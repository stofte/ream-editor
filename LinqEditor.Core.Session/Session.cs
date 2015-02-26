using LinqEditor.Core.CodeAnalysis.Compiler;
using LinqEditor.Core.CodeAnalysis.Services;
using LinqEditor.Core.Containers;
using LinqEditor.Core.Helpers;
using LinqEditor.Core.Models.Analysis;
using LinqEditor.Core.Models.Database;
using LinqEditor.Core.Models.Editor;
using LinqEditor.Core.Schema.Services;
using LinqEditor.Core.Settings;
using LinqEditor.Core.Templates;
using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace LinqEditor.Core.Session
{
    /// <summary>
    /// well designed class with single responsibility of doing everything
    /// </summary>
    public class Session : ISession, IDisposable
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

        public Session(Guid contextId, ISqlSchemaProvider schemaProvider, ITemplateService generator,
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
            _watch.Restart();
            if (_initialized) throw new InvalidOperationException("Cannot initialize more then once");
            _codeSession = connectionId == ConnectionStore.CodeId;
            _initialized = true;
            DebugLogger.Log("connectionId " + connectionId);
            return _codeSession ? InitCode() : InitDatabase(connectionId);
        }

        public InitializeResult Reinitialize()
        {
            if (!_canReinitialize) throw new InvalidOperationException("Cannot reinitialize unless execute was cancelled");
            _canReinitialize = false;
            DebugLogger.Log("_codeSession " + _codeSession);
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

            // map line info to sourceFragment offset
            result.Errors = result.Errors.Select(x => new Error 
            { 
                Location = x.Location.Offset(-_codeAnalysis.IndexOffset, -_codeAnalysis.LineOffset), 
                Message = x.Message 
            });
            result.Warnings = result.Warnings.Select(x => new Warning
            {
                Location = x.Location.Offset(-_codeAnalysis.IndexOffset, -_codeAnalysis.LineOffset),
                Message = x.Message
            });

            if (result.Success)
            {
                // this is probably wrong
                ct.Register(() => 
                {
                    _canReinitialize = true;
                    if (_codeSession)
                    {
                        DebugLogger.Log("ct.Register disposing code " + _codeContainer.Id);
                        _codeContainer.Dispose();
                        _codeContainerFactory.Release(_codeContainer);
                    }
                    else
                    {
                        DebugLogger.Log("ct.Register disposing db " + _databaseContainer.Id);
                        _databaseContainer.Dispose();
                        _databaseContainerFactory.Release(_databaseContainer);
                    }
                });

                ExecuteResult containerResult = null;

                try
                {
                    DebugLogger.Log("AssemblyBytes.GetHashCode " + result.AssemblyBytes.GetHashCode());

                    _prevHash = result.AssemblyBytes.GetHashCode();
                    _prevSrc = sourceFragment;

                    containerResult = _codeSession ? _codeContainer.Value.Execute(result.AssemblyBytes) :
                    _databaseContainer.Value.Execute(result.AssemblyBytes);
                }
                catch (AppDomainUnloadedException exn)
                {
                    DebugLogger.Log("AppDomainUnloadedException " + exn.ToString());

                    return new ExecuteResult
                    {
                        Success = false,
                        Kind = _codeSession ? ProgramType.Code : ProgramType.Database,
                        Cancelled = true,
                        DurationMs = _watch.ElapsedMilliseconds,
                    };
                }

                DebugLogger.Log("containerResult.Success " + containerResult.Success);
                DebugLogger.Log("containerResult.CodeOutput " + containerResult.CodeOutput);

                return new ExecuteResult
                {
                    Success = containerResult.Success,
                    QueryText = containerResult.QueryText,
                    Tables = containerResult.Tables,
                    Warnings = result.Warnings,
                    Errors = result.Errors,
                    DurationMs = _watch.ElapsedMilliseconds,
                    CodeOutput = containerResult.CodeOutput,
                    Kind = _codeSession ? ProgramType.Code : ProgramType.Database,
                };
            }

            return new ExecuteResult
            {
                Success = false,
                Errors = result.Errors,
                Warnings = result.Warnings,
                DurationMs = _watch.ElapsedMilliseconds,
                Kind = _codeSession ? ProgramType.Code : ProgramType.Database,
            };
        }

        public LoadAppDomainResult LoadAppDomain()
        {
            _watch.Restart();
            if (_loadedAppDomain) throw new InvalidOperationException("Cannot load AppDomain more then once, unless execute was cancelled");
            Exception exn = null;
            // loads schema in new appdomain
            if (_codeSession)
            {
                _codeContainer = _codeContainerFactory.Create();
                var res = _codeContainer.Value.Initialize();
                exn = res.Error;
                DebugLogger.Log("created code " + _codeContainer.Id);
            }
            else
            {
                _databaseContainer = _databaseContainerFactory.Create();
                var res = _databaseContainer.Value.Initialize(_connection.CachedSchemaFileName);
                exn = res.Error;
                DebugLogger.Log("created db " + _databaseContainer.Id);
            }

            _loadedAppDomain = true;

            return new LoadAppDomainResult
            {
                DurationMs = _watch.ElapsedMilliseconds,
                Error = exn, // only member set in runner
            };
        }
        
        // expose code analysis
        public AnalysisResult Analyze(string sourceFragment, int updateIndex)
        {
            if (!_codeAnalysis.IsReady)
            {
                return new AnalysisResult { Context = UserContext.NotReady };
            }
            return _codeAnalysis.Analyze(sourceFragment, updateIndex);
        }

        public AnalysisResult Analyze(string sourceFragment)
        {
            if (!_codeAnalysis.IsReady)
            {
                return new AnalysisResult { Context = UserContext.NotReady };
            }
            return _codeAnalysis.Analyze(sourceFragment);
        }
        
        private InitializeResult InitCode()
        {
            _codeAnalysis.Initialize();
            return new InitializeResult()
            {
                DurationMs = _watch.ElapsedMilliseconds,
            };
        }

        /// <summary>
        /// Initializes the database session by compiling the required schema assemblies.
        /// If there is no cached schema, one is obtained from the connection string.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private InitializeResult InitDatabase(Guid id)
        {
            _connection = _connectionStore.Connections.Where(x => x.Id == id).SingleOrDefault();
            if (_connection == null) throw new ArgumentException("unknown connection id");

            // check cache
            if (string.IsNullOrEmpty(_connection.CachedSchemaFileName) ||
                !System.IO.File.Exists(_connection.CachedSchemaFileName))
            {
                var schemaId = Guid.NewGuid();
                var schemaNamespace = schemaId.ToIdentifierWithPrefix(SchemaConstants.SchemaPrefix);

                // network access
                DatabaseSchema sqlSchema = null;
                try
                {
                    sqlSchema = _schemaProvider.GetSchema(_connection.ConnectionString);
                }
                catch(SqlException exn)
                {
                    return new InitializeResult
                    {
                        Error = exn,
                        DurationMs = _watch.ElapsedMilliseconds
                    };
                }
                
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
                // this shouldn't be needed anymore
                AssemblyPath = _connection.CachedSchemaFileName,
                SchemaNamespace = _connection.CachedSchemaNamespace,
                DurationMs = _watch.ElapsedMilliseconds
            };
        }

        public void Dispose()
        {
            DebugLogger.Log("Disposing session");
        }
    }
}
