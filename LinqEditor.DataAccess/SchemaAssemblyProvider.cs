using LinqEditor.Core.Models.Database;
using LinqEditor.Core.Settings;
using System;
using System.Threading.Tasks;

namespace LinqEditor.DataAccess
{
    public class SchemaAssemblyProvider : ISchemaAssemblyProvider
    {
        public async Task<DatabaseSchema> Load(Connection connection)
        {
            return new DatabaseSchema();
        }
    }
}
