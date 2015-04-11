using LinqEditor.Core.Models;
using LinqEditor.Core.Models.Database;
using LinqEditor.Core.Settings;

namespace LinqEditor.Core.Schema.Services
{
    public interface ISchemaProvider
    {
        DatabaseSchema GetSchema(Connection connection);
    }

    public interface ISqlServerSchemaProvider : ISchemaProvider { }
    public interface ISQLiteSchemaProvider : ISchemaProvider { }
}
