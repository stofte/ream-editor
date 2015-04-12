using LinqEditor.Core.Models;
using LinqEditor.Core.Models.Database;
using LinqEditor.Core.Settings;
using System.Threading.Tasks;

namespace LinqEditor.Schema
{
    public interface IServerSchemaProvider<T> : IDatabaseSchemaProvider<T>
    {
        Task<ServerSchema> GetServerSchema(T connection);
    }
}
