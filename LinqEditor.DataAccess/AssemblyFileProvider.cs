using LinqEditor.Core.Models;
using LinqEditor.Core.Settings;
using System;
using System.Threading.Tasks;

namespace LinqEditor.DataAccess
{
    public class AssemblyFileProvider : IAssemblyFileProvider
    {
        IConnectionStore _connectionStore;

        public AssemblyFileProvider(IConnectionStore connectionStore)
        {
            if (connectionStore == null)
            {
                throw new ArgumentNullException("connectionStore");
            }

            _connectionStore = connectionStore;
        }

        public async Task<string> GetSchemaPath(Connection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            var sqlServer = connection as SqlServerConnection;

            if (!sqlServer.IsValidConnectionString)
            {
                throw new ArgumentException("connection must contain valid connection string");
            }

            return string.Empty;
        }
    }
}
