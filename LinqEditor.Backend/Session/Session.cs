using Castle.Core;
using IQToolkit;
using IQToolkit.Data;
using LinqEditor.Backend.Compiler;
using LinqEditor.Backend.Interfaces;
using LinqEditor.Backend.Isolated;
using LinqEditor.Generator.Interfaces;
using LinqEditor.Schema.Interface;
using LinqEditor.Schema.Model;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Backend.Session
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
        private ISchemaProvider _schemaProvider;
        private IGenerator _generator;

        //private DbEntityProvider _entityProvider;

        private Isolated<QueryContainer> _container;

        public Session(string connectionString, ICSharpCompiler compiler, ISchemaProvider schemaProvider, IGenerator generator)
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
                var result = _compiler.Compile(schemaSource, _schemaNamespace);
                _schemaPath = result.CompiledAssemblyPath;
                // loads schema in new appdomain
                _container = new Isolated<QueryContainer>();
                _container.Value.Initialize(_schemaPath, _connectionString);

            });
        }

        public async Task<IEnumerable<DataTable>> Execute(string sourceFragment)
        {
            return await Task.Run(() =>
            {
                string queryNamespace;
                var querySource = _generator.GenerateQuery(_queryId++, out queryNamespace, sourceFragment, _schemaNamespace);
                var result = _compiler.Compile(querySource, queryNamespace, _schemaPath);
                return _container.Value.Execute(result.CompiledAssemblyImage);
            });
        }

        public void Dispose()
        {
            _container.Dispose();
        }
    }
}
