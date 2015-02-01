using IQToolkit.Data;
using LinqEditor.Common.Generated;
using LinqEditor.Core.Backend.Models;
using System;
using System.IO;
using System.Reflection;

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
            // i dont get how the runtime actually loads assemblies, so for now, we just grab
            // the resolve failed event, and return the passed assembly straight. this avoid
            // having to use the probe-path configuration setup, and lets us persist assemblies
            // where ever

            AppDomain.CurrentDomain.AssemblyResolve += delegate(object sender, ResolveEventArgs args)
            {
                if (args.RequestingAssembly != null)
                {
                    return _initialAssembly;
                }
                return null;
            };

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

                // code context depends on initial connection string
                _runnerType = connectionString != null ? ProgramType.Database : ProgramType.Code;

                if (_runnerType == ProgramType.Database)
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
                System.Threading.Thread.Sleep(2000);
                var queryType = assm.GetType(string.Format("{0}.Program", assm.GetName().Name));

                if (_runnerType == ProgramType.Database)
                {
                    var instance = Activator.CreateInstance(queryType) as IDatabaseProgram;
                    return ExecuteDatabase(instance);
                }
                else // _runnerType == ProgramType.Code
                {
                    var instance = Activator.CreateInstance(queryType) as ICodeProgram;
                    return ExecuteCode(instance);
                }
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
            var output = instance.Execute();
            return new ExecuteResult
            {
                CodeOutput = output
            };
        }

        // http://blogs.microsoft.co.il/sasha/2008/07/19/appdomains-and-remoting-life-time-service/
        public override object InitializeLifetimeService()
        {
            return null;
        }
    }
}
 