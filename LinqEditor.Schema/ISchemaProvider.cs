using LinqEditor.Core.Models;
using LinqEditor.Core.Models.Database;
using System.Threading.Tasks;

namespace LinqEditor.Schema
{
    public interface ISchemaProvider
    {
        Task<ServerSchema> GetServerSchema(SqlServerConnection connection);
        Task<DatabaseSchema> GetDatabaseSchema(SqlServerConnection connection);
        Task<DatabaseSchema> GetDatabaseSchema(SQLiteFileConnection connection);
    }
}
