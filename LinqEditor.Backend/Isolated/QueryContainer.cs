using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IQToolkit;
using IQToolkit.Data;
using IQToolkit.Data.SqlClient;
using System.Reflection;
using LinqEditor.Backend.Interfaces;

namespace LinqEditor.Backend.Isolated
{
    public class QueryContainer : MarshalByRefObject
    {
        private Assembly _schema;
        private string _dbType;
        private string _connectionString;

        public void Initialize(string schemaAssemblyPath, string connectionString)
        {
            _schema = Assembly.LoadFile(schemaAssemblyPath);
            _dbType = string.Format("{0}.DatabaseWithAttributes", _schema.GetTypes()[0].Namespace);
            _connectionString = connectionString;
        }

        public IEnumerable<DataTable> Execute(string queryAssemblyPath)
        {
            var queryAssembly = Assembly.LoadFile(queryAssemblyPath);
            var queryType = queryAssembly.GetType(string.Format("{0}.Program", queryAssembly.GetTypes()[0].Namespace));
            var instance = Activator.CreateInstance(queryType) as IQueryUnit;
            var provider = DbEntityProvider.From("IQToolkit.Data.SqlClient", _connectionString, _dbType);
            provider.Connection.Open();
            var dumps = instance.Execute(provider);
            provider.Connection.Close();
            return dumps;
        }
    }
}
