using LinqEditor.Core.Models.Database;
using LinqEditor.Core.Settings;
using System.Threading.Tasks;

namespace LinqEditor.Schema
{
    public interface ISchemaProvider
    {
        Task<ServerSchema> GetSchema(Connection connection);
    }

    // for now, we just denote each supported provider by subtyping the base interface, 
    // and binding against the subinterface in the constructor itself.
    public interface ISqlServerSchemaProvider : ISchemaProvider { }
    public interface ISQLiteSchemaProvider : ISchemaProvider { }
}
