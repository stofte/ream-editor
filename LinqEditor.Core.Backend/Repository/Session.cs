using LinqEditor.Core.Models.Editor;
using LinqEditor.Core.Backend.Settings;
using LinqEditor.Core.CodeAnalysis.Compiler;
using LinqEditor.Core.Context;
using LinqEditor.Core.Helpers;
using LinqEditor.Core.Schema.Services;
using LinqEditor.Core.Templates;
using System;
using System.Diagnostics;
using System.IO;

namespace LinqEditor.Core.Backend.Repository
{
    public class Session : ISession, IDisposable
    {
        private string _connectionString;
        private string _schemaPath;
        private string _schemaNamespace;
        private string _outputFolder;
        private Guid _sessionId = Guid.NewGuid();

        private ISqlSchemaProvider _schemaProvider;
        private ITemplateService _generator;
        private ISchemaStore _userSettings;
        private IContext _context;
        private Stopwatch _watch;

        private Core.Containers.Isolated<LinqEditor.Core.Containers.DatabaseContainer> _container;

        public Session(ISqlSchemaProvider schemaProvider, ITemplateService generator, ISchemaStore userSettings, IContext context)
        {
            _schemaProvider = schemaProvider;
            _generator = generator;
            _userSettings = userSettings;
            _context = context;
            _watch = new Stopwatch();
            _outputFolder = Core.PathUtility.CachePath;
        }

        public InitializeResult Initialize(string connectionString)
        {
            _connectionString = connectionString;
            // check cache
            _schemaPath = _userSettings.GetCachedAssembly(_connectionString);
            if (string.IsNullOrEmpty(_schemaPath))
            {
                _schemaNamespace = _sessionId.ToIdentifierWithPrefix("s");
                var sqlSchema = _schemaProvider.GetSchema(_connectionString);
                var schemaSource = _generator.GenerateSchema(_sessionId, sqlSchema);
                var result = CSharpCompiler.CompileToFile(schemaSource, _schemaNamespace, _outputFolder);
                _schemaPath = result.AssemblyPath;

                if (result.Success)
                {
                    _userSettings.PersistSchema(_connectionString, sqlSchema, _schemaPath);
                }
            }
            else
            {
                // todo: probably want to store namespace in settings also
                _schemaNamespace = Path.GetFileNameWithoutExtension(_schemaPath);
            }

            _context.UpdateContext(_schemaPath, _schemaNamespace);

            return new InitializeResult
            {
                AssemblyPath = _schemaPath,
                SchemaNamespace = _schemaNamespace
            };
        }

        public ExecuteResult Execute(string sourceFragment)
        {
            _watch.Restart();
            var queryId = Guid.NewGuid();
            var querySource = _generator.GenerateQuery(queryId, sourceFragment, _schemaNamespace);
            var result = CSharpCompiler.CompileToBytes(querySource, queryId.ToIdentifierWithPrefix("q"), _schemaPath);

            if (result.Success)
            {
                var containerResult = _container.Value.Execute(result.AssemblyBytes);
                _watch.Stop();

                return new ExecuteResult
                {
                    Success = containerResult.Success,
                    QueryText = containerResult.QueryText,
                    Tables = containerResult.Tables,
                    Warnings = result.Warnings,
                    Duration = _watch.ElapsedMilliseconds
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
            // loads schema in new appdomain
            _container = new Core.Containers.Isolated<LinqEditor.Core.Containers.DatabaseContainer>();
            var initResult = _container.Value.Initialize(_schemaPath, _connectionString);

            return new LoadAppDomainResult
            {
                Error = initResult.Error, // only member set in runner
            };
        }

        public void Dispose()
        {
            _container.Dispose();
        }
    }
}
