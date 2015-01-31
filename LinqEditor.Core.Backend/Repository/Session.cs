using LinqEditor.Core.Templates;
using LinqEditor.Core.Backend.Isolated;
using LinqEditor.Core.Schema.Models;
using LinqEditor.Core.Schema.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqEditor.Core.CodeAnalysis.Compiler;
using LinqEditor.Core.Backend.Models;
using LinqEditor.Core.CodeAnalysis.Models;
using LinqEditor.Core.Backend.Settings;
using LinqEditor.Core.Schema.Helpers;
using System.IO;
using System.Diagnostics;

namespace LinqEditor.Core.Backend.Repository
{
    public class Session : ISession, IDisposable
    {
        private string _connectionString;
        private string _schemaPath;
        private string _schemaNamespace;
        private string _outputFolder;
        private Guid _sessionId = Guid.NewGuid();

        private ICSharpCompiler _compiler;
        private ISqlSchemaProvider _schemaProvider;
        private ITemplateService _generator;
        private ISchemaStore _userSettings;
        private Stopwatch _watch;

        private Isolated<Runner> _container;

        public Session(ICSharpCompiler compiler, ISqlSchemaProvider schemaProvider, ITemplateService generator, ISchemaStore userSettings)
        {
            _compiler = compiler;
            _schemaProvider = schemaProvider;
            _generator = generator;
            _userSettings = userSettings;
            _watch = new Stopwatch();
            _outputFolder = Common.Utility.CachePath();
        }

        public InitializeResult Initialize(string connectionString)
        {
            _connectionString = connectionString;
            // check cache
            _schemaPath = _userSettings.GetCachedAssembly(_connectionString);
            if (string.IsNullOrEmpty(_schemaPath))
            {
                var sqlSchema = _schemaProvider.GetSchema(_connectionString);
                var schemaSource = _generator.GenerateSchema(_sessionId, sqlSchema);
                var result = _compiler.Compile(schemaSource, _sessionId.ToIdentifierWithPrefix("s"), _outputFolder);
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
            var result = _compiler.Compile(querySource, queryId.ToIdentifierWithPrefix("q"), _outputFolder, references: _schemaPath);

            if (result.Success)
            {
                var containerResult = _container.Value.Execute(result.AssemblyPath);
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
            _container = new Isolated<Runner>();
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
