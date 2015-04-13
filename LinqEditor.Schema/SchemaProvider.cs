using LinqEditor.Core.Models;
using LinqEditor.Core.Models.Database;
using System;
using System.Threading.Tasks;

namespace LinqEditor.Schema
{
    public class SchemaProvider : ISchemaProvider
    {
        ISQLiteSchemaProvider _sqliteProvider;
        ISqlServerSchemaProvider _sqlServerProvider;

        public SchemaProvider(ISQLiteSchemaProvider sqliteProvider, ISqlServerSchemaProvider sqlServerProvider)
        {
            if (sqliteProvider == null) throw new ArgumentNullException("sqliteProvider");
            if (sqlServerProvider == null) throw new ArgumentNullException("sqlServerProvider");
            _sqliteProvider = sqliteProvider;
            _sqlServerProvider = sqlServerProvider;
        }

        public async Task<ServerSchema> GetServerSchema(SqlServerConnection connection)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            return await _sqlServerProvider.GetServerSchema(connection);
        }

        public async Task<DatabaseSchema> GetDatabaseSchema(SqlServerConnection connection)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            return await _sqlServerProvider.GetDatabaseSchema(connection);
        }

        public async Task<DatabaseSchema> GetDatabaseSchema(SQLiteFileConnection connection)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            return await _sqliteProvider.GetDatabaseSchema(connection);
        }
    }
}
