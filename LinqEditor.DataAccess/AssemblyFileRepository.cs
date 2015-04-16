using LinqEditor.Core.Helpers;
using LinqEditor.Core.Models;
using LinqEditor.Core.Models.Database;
using LinqEditor.Core.Settings;
using LinqEditor.Schema;
using System;
using System.Threading.Tasks;

namespace LinqEditor.DataAccess
{
    public class AssemblyFileRepository : IAssemblyFileRepository
    {
        ISchemaProvider _schemaProvider;
        IConnectionStore _connectionStore;

        public AssemblyFileRepository(ISchemaProvider schemaProvider, IConnectionStore connectionStore)
        {
            if (schemaProvider == null) throw new ArgumentNullException("schemaProvider");
            if (connectionStore == null) throw new ArgumentNullException("connectionStore");
            _schemaProvider = schemaProvider;
            _connectionStore = connectionStore;
        }

        public async Task<string> GetAssemblyFilePath(Connection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (!connection.IsValidConnectionString)
            {
                throw new ArgumentException("connection must contain valid connection string");
            }

            var sqlServer = connection as SqlServerConnection;
            var sqlite = connection as SQLiteFileConnection;

            DatabaseSchema dbSchema = null;
            ServerSchema serverSchema = null;

            if (sqlServer != null)
            {
                if (!string.IsNullOrWhiteSpace(sqlServer.InitialCatalog))
                {
                    dbSchema = await _schemaProvider.GetDatabaseSchema(sqlServer);
                }
                else
                {
                    serverSchema = await _schemaProvider.GetServerSchema(sqlServer);
                }
            }
            else if (sqlite != null)
            {
                dbSchema = await _schemaProvider.GetDatabaseSchema(sqlite);
            }

            var o = dbSchema as object ?? serverSchema as object;

            if (o == null) throw new ApplicationException("schema was null");

            // update the connectionstore with the latest known hash
            connection.SchemaHash = SerializationHelper.Hash(o);
            _connectionStore.Update(connection);

            return string.Empty;
        }
    }
}
