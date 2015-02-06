using LinqEditor.Core.Models.Database;

namespace LinqEditor.Core.Settings
{
    public interface ISchemaStore
    {
        string GetCachedAssembly(string connectionString);
        void PersistSchema(string connectionString, DatabaseSchema schema, string assemblyPath);
    }
}
