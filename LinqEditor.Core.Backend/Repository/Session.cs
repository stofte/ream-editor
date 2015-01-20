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

namespace LinqEditor.Core.Backend.Repository
{
    public class Session : ISession, IDisposable
    {
        private static int SessionCounter = 0;
        private string _connectionString;
        private IEnumerable<TableSchema> _sqlTables;
        private string _schemaPath;
        private string _schemaNamespace;
        private int _sessionId = SessionCounter++; // should be ok
        private int _queryId = 0;

        //private AppDomain _executionDomain;

        private ICSharpCompiler _compiler;
        private ISqlSchemaProvider _schemaProvider;
        private ITemplateService _generator;

        //private DbEntityProvider _entityProvider;

        private Isolated<AppDomainProxy> _container;

        public Session(string connectionString, ICSharpCompiler compiler, ISqlSchemaProvider schemaProvider, ITemplateService generator)
        {
            _connectionString = connectionString;
            _compiler = compiler;
            _schemaProvider = schemaProvider;
            _generator = generator;
        }

        public async Task Initialize()
        {
            // ok to use Task.Run when "just" offloading from UI thread
            // http://blogs.msdn.com/b/pfxteam/archive/2012/04/12/10293335.aspx?Redirected=true
            await Task.Run(() =>
            {
                _sqlTables = _schemaProvider.GetSchema(_connectionString);
                _schemaNamespace = "";
                var schemaSource = _generator.GenerateSchema(_sessionId, out _schemaNamespace, _sqlTables);
                var result = _compiler.Compile(schemaSource, _schemaNamespace, generateFiles: true);
                _schemaPath = result.AssemblyPath;
                // loads schema in new appdomain
                _container = new Isolated<AppDomainProxy>();
                _container.Value.Initialize(_schemaPath, _connectionString);

            });
        }

        public async Task<IEnumerable<DataTable>> Execute(string sourceFragment)
        {
            return await Task.Run(() =>
            {
                string queryNamespace;
                var querySource = _generator.GenerateQuery(_queryId++, out queryNamespace, sourceFragment, _schemaNamespace);
                var result = _compiler.Compile(querySource, queryNamespace, generateFiles: false, references: _schemaPath);
                return _container.Value.Execute(result.AssemblyBytes);
            });
        }

        public void Dispose()
        {
            _container.Dispose();
        }
    }
}
