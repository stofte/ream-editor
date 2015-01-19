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
        private int _sessionId = SessionCounter++; // should be ok
        private int _queryId = 0;

        //private AppDomain _executionDomain;

        private ICSharpCompiler _compiler;
        private ISchemaProvider _schemaProvider;
        private ISchemaGenerator _schemaGenerator;
        private IQueryGenerator _queryGenerator;
        //private DbEntityProvider _entityProvider;

        private Isolated<QueryContainer> _container;

        public Session(string connectionString, ICSharpCompiler compiler, ISchemaProvider schemaProvider, ISchemaGenerator schemaGenerator, IQueryGenerator queryGenerator)
        {
            _connectionString = connectionString;
            _compiler = compiler;
            _schemaProvider = schemaProvider;
            _schemaGenerator = schemaGenerator;
            _queryGenerator = queryGenerator;
        }

        public async Task Initialize()
        {
            // ok to use Task.Run when "just" offloading from UI thread
            // http://blogs.msdn.com/b/pfxteam/archive/2012/04/12/10293335.aspx?Redirected=true
            await Task.Run(() => 
            {
                _sqlTables = _schemaProvider.GetSchema(_connectionString);
                _schemaGenerator.NamespaceId = _sessionId;
                _schemaGenerator.Tables = _sqlTables;
                var schemaSource = _schemaGenerator.TransformText();
                var result = _compiler.Compile(schemaSource, _schemaGenerator.GeneratedSchemaNamespace);

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
                var queryNs = string.Format("q{0}", _queryId++);
                var querySource = @"
using System;
using System.IO;
using System.Linq;
using System.Data;
using System.Collections.Generic;
using IQToolkit;
using IQToolkit.Data;
using IQToolkit.Data.Mapping;
using LinqEditor.Backend.Dumper;
using LinqEditor.Backend.Interfaces;
using " + _schemaGenerator.GeneratedSchemaNamespace + @".Schema;

namespace " + queryNs + @" {

    public class Program : ProgramBase
    {
        protected override void Query() 
        {
" + sourceFragment + @"
        }
    }

}";
                //_queryGenerator.SourceCode = sourceFragment;
                //_queryGenerator.NamespaceId = _queryId++;
                //_queryGenerator.GeneratedSchemaNamespace = _schemaGenerator.GeneratedSchemaNamespace;
                //_queryGenerator.Tables = _sqlTables;
                //var querySource = _queryGenerator.TransformText();
                var result = _compiler.Compile(querySource, queryNs, _schemaPath);

                return _container.Value.Execute(result.CompiledAssemblyPath);
            });
        }

        public void Dispose()
        {
            _container.Dispose();
        }
    }
}
