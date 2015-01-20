using IQToolkit.Data;
using LinqEditor.Core.CodeAnalysis.Injected;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.Backend.Isolated
{
    public class AppDomainProxy : MarshalByRefObject
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

        public IEnumerable<DataTable> Execute(byte[] assembly)
        {
            var assm = Assembly.Load(assembly);
            return Execute(assm);
        }

        public IEnumerable<DataTable> Execute(string queryAssemblyPath)
        {
            var assm = Assembly.LoadFile(queryAssemblyPath);
            return Execute(assm);
        }

        private IEnumerable<DataTable> Execute(Assembly assm)
        {
            var queryType = assm.GetType(string.Format("{0}.Program", assm.GetTypes()[0].Namespace));
            var instance = Activator.CreateInstance(queryType) as IDynamicQuery;
            var provider = DbEntityProvider.From("IQToolkit.Data.SqlClient", _connectionString, _dbType);
            provider.Connection.Open();
            var dumps = instance.Execute(provider);
            provider.Connection.Close();
            return dumps;
        }

        // http://blogs.microsoft.co.il/sasha/2008/07/19/appdomains-and-remoting-life-time-service/
        public override object InitializeLifetimeService()
        {   
            return null;
        }
    }
}
