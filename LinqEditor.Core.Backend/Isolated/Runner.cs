using IQToolkit.Data;
using LinqEditor.Core.Backend.Models;
using LinqEditor.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using LinqEditor.Common.Generated;

namespace LinqEditor.Core.Backend.Isolated
{
    public class Runner : MarshalByRefObject
    {
        private Assembly _initialAssembly;
        private ProgramType _runnerType;
        private string _dbType;
        private string _connectionString;

        public LoadAppDomainResult Initialize(byte[] assemblyImage, string connectionString = null)
        {
            return Initialize(assemblyImage, null, connectionString);
        }

        public LoadAppDomainResult Initialize(string assemblyPath, string connectionString = null)
        {
            return Initialize(null, assemblyPath, connectionString);
        }

        private LoadAppDomainResult Initialize(byte[] assemblyImage, string assemblyPath, string connectionString)
        {
            Exception exn = null;
            try
            {
                if (!string.IsNullOrEmpty(assemblyPath))
                {
                    _initialAssembly = Assembly.LoadFile(assemblyPath);
                }
                else
                {
                    _initialAssembly = Assembly.Load(assemblyImage);
                }

                _runnerType = connectionString != null ? ProgramType.Database : ProgramType.Code;

                if (connectionString != null)
                {
                    _dbType = string.Format("{0}.Schema.DatabaseWithAttributes", _initialAssembly.GetName().Name);
                    _connectionString = connectionString;

                    // warm up connection
                    var warmupType = _initialAssembly.GetType(string.Format("{0}.Schema.WarmUpConnection", _initialAssembly.GetName().Name));
                    var instance = Activator.CreateInstance(warmupType) as IDatabaseProgram;
                    var res = ExecuteDatabase(instance);
                }
            }
            catch (Exception e)
            {
                exn = e;
            }

            return new LoadAppDomainResult
            {
                Error = exn
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
                var queryType = assm.GetType(string.Format("{0}.Program", assm.GetName().Name));

                //if (_runnerType == ProgramType.Database)
                //{
                    var instance = Activator.CreateInstance(queryType) as IDatabaseProgram;
                    return ExecuteDatabase(instance);
                //}
                //else if (_runnerType == ProgramType.Code)
                //{
                //    var instance = Activator.CreateInstance(queryType) as ICodeProgram;
                //}
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


        private ExecuteResult ExecuteDatabase(IDatabaseProgram instance)
        {
            var provider = DbEntityProvider.From("IQToolkit.Data.SqlClient", _connectionString, _dbType);
            provider.Log = new StringWriter();
            provider.Connection.Open();
            var dumps = instance.Execute(provider);
            provider.Connection.Close();

            return new ExecuteResult
            {
                Tables = dumps,
                Success = true,
                QueryText = provider.Log.ToString()
            };
        }

        private ExecuteResult ExecuteCode(ICodeProgram instance)
        {
            return new ExecuteResult
            {
                
            };
        }

        // http://blogs.microsoft.co.il/sasha/2008/07/19/appdomains-and-remoting-life-time-service/
        public override object InitializeLifetimeService()
        {
            return null;
        }
    }
}
 