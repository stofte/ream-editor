using IQToolkit.Data;
using LinqEditor.Core.Generated;
using LinqEditor.Core.Models.Editor;
using System;
using System.IO;
using System.Reflection;

namespace LinqEditor.Core.Containers
{
    public class DatabaseContainer : ExecutionContainer
    {
        private string _dbType;
        private string _connectionString;

        public LoadAppDomainResult Initialize(byte[] assembly, string connectionString)
        {
            return Initialize(assembly, null, connectionString);
        }

        public LoadAppDomainResult Initialize(string assemblyPath, string connectionString)
        {
            return Initialize(null, assemblyPath, connectionString);
        }

        public ExecuteResult Execute(byte[] assembly)
        {
            return Execute(assembly, null);
        }

        public ExecuteResult Execute(string path)
        {
            return Execute(null, path);
        }

        private LoadAppDomainResult Initialize(byte[] assemblyImage, string assemblyPath, string connectionString)
        {
            // probably want to make this try/catch build dependent?
            Exception exn = null;
            try
            {
                if (!string.IsNullOrEmpty(assemblyPath))
                {
                    _baseAssembly = Assembly.LoadFile(assemblyPath);
                }
                else
                {
                    _baseAssembly = Assembly.Load(assemblyImage);
                }

                // code context depends on initial connection string
                _runnerType = connectionString != null ? ProgramType.Database : ProgramType.Code;

                if (_runnerType == ProgramType.Database)
                {
                    _dbType = string.Format("{0}.Schema.DatabaseWithAttributes", _baseAssembly.GetName().Name);
                    _connectionString = connectionString;

                    // warm up connection
                    var warmupType = _baseAssembly.GetType(string.Format("{0}.Schema.WarmUpConnection", _baseAssembly.GetName().Name));
                    var instance = Activator.CreateInstance(warmupType) as IDatabaseProgram;
                    var res = ExecuteInstance(instance);
                }
                base.InitializeAppDomain();
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

        private ExecuteResult Execute(byte[] assembly, string path)
        {
            var assm = !string.IsNullOrEmpty(path) ? Assembly.LoadFile(path) : Assembly.Load(assembly);
            var queryType = assm.GetType(string.Format("{0}.Program", assm.GetName().Name));

            var instance = Activator.CreateInstance(queryType) as IDatabaseProgram;
            return ExecuteInstance(instance);
        }

        private ExecuteResult ExecuteInstance(IDatabaseProgram instance)
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

    }
}
