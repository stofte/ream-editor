using LinqEditor.Core.Models;
using LinqEditor.Core.Settings;
using System;
using System.Threading.Tasks;

namespace LinqEditor.DataAccess
{
    public class AssemblyFileRepository : IAssemblyFileRepository
    {
        public async Task<string> GetAssemblyFilePath(Connection connection)
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
