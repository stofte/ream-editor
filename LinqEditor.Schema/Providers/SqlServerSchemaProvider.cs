using LinqEditor.Core.Models;
using LinqEditor.Core.Models.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace LinqEditor.Schema.Providers
{
    public class SqlServerSchemaProvider : ISqlServerSchemaProvider
    {
        static IEnumerable<string> SystemDatabases = new [] {"master", "tempdb", "model", "msdb"};

        public async Task<ServerSchema> GetServerSchema(SqlServerConnection connection)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            var hasInitialCatalog = !string.IsNullOrWhiteSpace(connection.InitialCatalog);

            var result = new List<DatabaseSchema>();
            using (var conn = new SqlConnection(connection.ConnectionString))
            {
                await conn.OpenAsync();
                using (var dbs = conn.GetSchema(SqlClientMetaDataCollectionNames.Databases))
                {
                    var dbNames = new List<string>();
                    foreach(DataRow row in dbs.Rows) 
                    {
                        var dbName = row.ItemArray[0] as string;
                        if (!hasInitialCatalog && !SystemDatabases.Contains(dbName) ||
                            hasInitialCatalog && (connection.InitialCatalog.ToLower() == dbName.ToLower()))
                        {
                            dbNames.Add(dbName);
                        }
                    }
                    var dbConns = connection.GetConnectionStringWithDatabases(dbNames).Select(x => new SqlServerConnection { ConnectionString = x });
                    foreach (var dbConn in dbConns)
                    {
                        result.Add(await GetDatabaseSchema(dbConn));
                    }
                }
            }

            return new ServerSchema
            {
                Databases = result
            };
        }

        public async Task<DatabaseSchema> GetDatabaseSchema(SqlServerConnection connection)
        {
            var tables = new List<TableSchema>();
            string dbName = string.Empty;

            using (var conn = new SqlConnection(connection.ConnectionString))
            {
                await conn.OpenAsync();
                using (var tableschema = conn.GetSchema(SqlClientMetaDataCollectionNames.Tables))
                {
                    // first column name
                    foreach (DataRow row in tableschema.Rows)
                    {
                        if (string.IsNullOrWhiteSpace(dbName))
                        {
                            dbName = row["TABLE_CATALOG"].ToString();
                        }
                        // all tables must have same catalog
                        Debug.Assert(dbName == row["TABLE_CATALOG"].ToString());
                        tables.Add(new TableSchema
                        {
                            Name = row["TABLE_NAME"].ToString(),
                            Catalog = row["TABLE_CATALOG"].ToString(),
                            Schema = row["TABLE_SCHEMA"].ToString()
                        });
                    }
                }

                foreach (var table in tables)
                {
                    using (var columnsSchema = conn.GetSchema("Columns", new string[] { table.Catalog, null, table.Name }))
                    {
                        var columns = new List<ColumnSchema>();
                        var index = 0;
                        foreach (DataRow row in columnsSchema.Rows)
                        {
                            columns.Add(new ColumnSchema
                            {
                                Name = row["COLUMN_NAME"].ToString(),
                                TypeName = "",
                                Type = MapSqlTypeToDotNetType(row["DATA_TYPE"].ToString(), row["IS_NULLABLE"].ToString()),
                                Index = index++
                            });
                        }
                        table.Columns = columns;
                    }
                }
            }
            return new DatabaseSchema 
            {
                Name = dbName,
                Tables = tables
            };
        }

        private Type MapSqlTypeToDotNetType(string type, string nullable)
        {
            var isNullable = nullable == "YES";
            var nullableSuffix = nullable == "YES" ? "?" : string.Empty;
            switch (type)
            {
                case "date":
                case "smalldatetime":
                case "datetime":
                case "datetime2":
                case "datetimeoffset":
                    return isNullable ? typeof(DateTime?) : typeof(DateTime);
                case "time":
                    return isNullable ? typeof(TimeSpan?) : typeof(TimeSpan);
                case "bit":
                    return isNullable ? typeof(bool?) : typeof(bool);
                case "decimal":
                case "money":
                case "smallmoney":
                    return isNullable ? typeof(decimal?) : typeof(decimal);
                case "bigint":
                case "numeric":
                    return isNullable ? typeof(long?) : typeof(long);
                case "int":
                case "smallint":
                case "tinyint":
                    return isNullable ? typeof(int?) : typeof(int);
                case "real":
                case "float":
                    return isNullable ? typeof(double?) : typeof(double);
                case "timestamp":
                case "rowversion":
                case "binary":
                case "varbinary":
                    return typeof(byte[]);
                case "uniqueidentifier":
                    return isNullable ? typeof(Guid?) : typeof(Guid);
                case "text":
                default:
                    return typeof(string);
                //default:
                //    throw new ArgumentException("Unsupported type");
            }
        }
    }
}
