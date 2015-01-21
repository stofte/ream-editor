using IQToolkit.Data;
using LinqEditor.Core.Backend.Models;
using LinqEditor.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.Backend.Isolated
{
    public class Runner : MarshalByRefObject
    {
        private Assembly _schema;
        private string _dbType;
        private string _connectionString;

        public InitializeResult Initialize(byte[] assemblyImage, string connectionString)
        {
            return Initialize(assemblyImage, null, connectionString);
        }

        public InitializeResult Initialize(string assemblyPath, string connectionString)
        {
            return Initialize(null, assemblyPath, connectionString);
        }

        private InitializeResult Initialize(byte[] assemblyImage, string assemblyPath, string connectionString)
        {
            Exception exn = null;
            try
            {
                if (!string.IsNullOrEmpty(assemblyPath))
                {
                    _schema = Assembly.LoadFile(assemblyPath);
                }
                else
                {
                    _schema = Assembly.Load(assemblyImage);
                }
                
                _dbType = string.Format("{0}.DatabaseWithAttributes", _schema.GetTypes()[0].Namespace);
                _connectionString = connectionString;
            }
            catch (Exception e)
            {
                exn = e;
            }

            return new InitializeResult
            {
                Exception = exn
            };
            
        }

        public ExecuteResult Execute(byte[] assembly)
        {
            return Execute(assembly, null);
        }

        public ExecuteResult Execute(string path)
        {
            return Execute(null, path);
        }

        private ExecuteResult Execute(byte[] assembly, string path)
        {
            try
            {
                var assm = !string.IsNullOrEmpty(path) ? Assembly.LoadFile(path) : Assembly.Load(assembly);

                var queryType = assm.GetType(string.Format("{0}.Program", assm.GetTypes()[0].Namespace));
                var instance = Activator.CreateInstance(queryType) as IDynamicQuery;
                var provider = DbEntityProvider.From("IQToolkit.Data.SqlClient", _connectionString, _dbType);
                provider.Connection.Open();
                var dumps = instance.Execute(provider);
                provider.Connection.Close();

                return new ExecuteResult
                {
                    Tables = dumps,
                    Success = true
                };
            }
            catch (Exception e)
            {

                return new ExecuteResult
                {
                    Exception = e,
                    Success = false
                };
            }
        }

        // http://blogs.microsoft.co.il/sasha/2008/07/19/appdomains-and-remoting-life-time-service/
        public override object InitializeLifetimeService()
        {   
            return null;
        }
    }
}
 