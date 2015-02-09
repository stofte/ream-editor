using LinqEditor.Core.Models.Database;
using System.Collections.Generic;

namespace LinqEditor.Core.Settings
{
    public interface ISchemaStore
    {
        string GetCachedAssembly(string connectionString);
        void PersistSchema(string connectionString, DatabaseSchema schema, string assemblyPath);
        void AddConnectionString(string connectionString, string name);
        void DeleteConnectionString(string connectionString);
        SerializableStringDictionary ConnectionNames { get; }
    }
}
