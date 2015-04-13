using LinqEditor.Core.Models;
using LinqEditor.Core.Settings;
using LinqEditor.Schema;
using System;
using System.Threading.Tasks;

namespace LinqEditor.DataAccess
{
    public class AssemblyFileRepository : IAssemblyFileRepository
    {
        ISchemaProvider _schemaProvider;

        public AssemblyFileRepository(ISchemaProvider schemaProvider)
        {
            if (schemaProvider == null) throw new ArgumentNullException("schemaProvider");
            _schemaProvider = schemaProvider;
        }

        public async Task<string> GetAssemblyFilePath(Connection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (!connection.IsValidConnectionString)
            {
                throw new ArgumentException("connection must contain valid connection string");
            }

            return string.Empty;
        }
    }
}
