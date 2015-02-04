using LinqEditor.Core.Models.Database;

namespace LinqEditor.Core.Schema.Services
{
    public interface ISqlSchemaProvider
    {
        DatabaseSchema GetSchema(string connectionString);
    }
}
