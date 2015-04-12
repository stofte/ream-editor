using LinqEditor.Core.Models;
using LinqEditor.Core.Models.Database;
using System.Threading.Tasks;

namespace LinqEditor.Schema
{
    public interface IDatabaseSchemaProvider<T>
    {
        Task<DatabaseSchema> GetDatabaseSchema(T connection);
    }
}
