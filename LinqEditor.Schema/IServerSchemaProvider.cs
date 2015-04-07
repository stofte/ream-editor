using LinqEditor.Core.Models.Database;
using LinqEditor.Core.Settings;
using System.Threading.Tasks;

namespace LinqEditor.Schema
{
    public interface IServerSchemaProvider : IDatabaseSchemaProvider
    {
        Task<ServerSchema> GetServerSchema(Connection connection);
    }
}
