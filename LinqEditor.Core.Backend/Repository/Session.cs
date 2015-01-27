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
using System.IO;

namespace LinqEditor.Core.Backend.Repository
{
    public class Session : ISession, IDisposable
    {
        private string _connectionString;
        private string _schemaPath;
        private string _schemaNamespace;
        private Guid _sessionId = Guid.NewGuid();
        private Guid _queryId = Guid.NewGuid();

        //private AppDomain _executionDomain;

        private ICSharpCompiler _compiler;
        private ISqlSchemaProvider _schemaProvider;
        private ITemplateService _generator;
        private ISchemaStore _userSettings;

        //private DbEntityProvider _entityProvider;

        private Isolated<Runner> _container;

        public Session(ICSharpCompiler compiler, ISqlSchemaProvider schemaProvider, ITemplateService generator, ISchemaStore userSettings)
        {
            _compiler = compiler;
            _schemaProvider = schemaProvider;
            _generator = generator;
            _userSettings = userSettings;
        }

        public InitializeResult Initialize(string connectionString)
        {
            _connectionString = connectionString;
            // check cache
            _schemaPath = _userSettings.GetCachedAssembly(_connectionString);
            if (string.IsNullOrEmpty(_schemaPath))
            {
                var sqlSchema = _schemaProvider.GetSchema(_connectionString);
                _schemaNamespace = "";
                var schemaSource = _generator.GenerateSchema(_sessionId, out _schemaNamespace, sqlSchema);
                var result = _compiler.Compile(schemaSource, _schemaNamespace, generateFiles: true);
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
            string queryNamespace;
            var querySource = _generator.GenerateQuery(_queryId, out queryNamespace, sourceFragment, _schemaNamespace);
            var result = _compiler.Compile(querySource, queryNamespace, generateFiles: false, references: _schemaPath);

            if (result.Success)
            {
                var containerResult = _container.Value.Execute(result.AssemblyBytes);

                return new ExecuteResult
                {
                    Success = containerResult.Success,
                    QueryText = containerResult.QueryText,
                    Tables = containerResult.Tables,
                    Warnings = result.Warnings
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
