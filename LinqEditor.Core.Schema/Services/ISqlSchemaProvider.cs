using LinqEditor.Core.Schema.Models;

namespace LinqEditor.Core.Schema.Services
{
    public interface ISqlSchemaProvider
    {
        DatabaseSchema GetSchema(string connectionString);
    }
}
