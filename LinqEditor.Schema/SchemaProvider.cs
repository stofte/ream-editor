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

        public async Task<ServerSchema> GetServerSchema(Connection connection)
        {
            var sqlServer = connection as SqlServerConnection;
            if (sqlServer != null)
            {
                return await _sqlServerProvider.GetServerSchema(sqlServer);
            }
            return null;
        }

        public async Task<DatabaseSchema> GetDatabaseSchema(Connection connection)
        {
            var sqlServer = connection as SqlServerConnection;
            var sqlite = connection as SQLiteFileConnection;
            if (sqlServer != null)
            {
                return await _sqlServerProvider.GetDatabaseSchema(sqlServer);
            }
            else if (sqlite != null)
            {
                return await _sqliteProvider.GetDatabaseSchema(sqlite);
            }
            return null;
        }
    }
}
