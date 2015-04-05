using LinqEditor.Core.Models.Database;
using LinqEditor.Core.Settings;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace LinqEditor.Core.Schema.Services
{
    public class SqlServerSchemaProvider : ISqlServerSchemaProvider
    {
        public DatabaseSchema GetSchema(Connection connection)
        {
            var tables = new List<TableSchema>();

            using (var conn = new SqlConnection(connection.ConnectionString))
            {
                conn.Open();

                // lacks async api?
                using (var tableschema = conn.GetSchema("Tables"))
                {
                    // first column name
                    foreach (DataRow row in tableschema.Rows)
                    {
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
                                Type = SqlTypeToDotNetType(row["DATA_TYPE"].ToString(), row["IS_NULLABLE"].ToString()),
                                Index = index++
                            });
                        }
                        table.Columns = columns;
                    }
                }
            }

            return new DatabaseSchema
            {
                Connection = connection,
                Tables = tables
            };
        }

        private string SqlTypeToDotNetType(string type, string nullable)
        {
            var nullableSuffix = nullable == "YES" ? "?" : string.Empty;
            switch (type)
            {
                case "date":
                case "smalldatetime":
                case "datetime":
                case "datetime2":
                    return "DateTime" + nullableSuffix;
                case "datetimeoffset":
                    return "DateTimeOffset" + nullableSuffix;
                case "time":
                    return "TimeSpan" + nullableSuffix;
                case "int":
                    return "int" + nullableSuffix;
                case "real":
                case "float":
                    return "double" + nullableSuffix;
                case "timestamp":
                case "rowversion":
                case "binary":
                case "varbinary":
                    return "byte[]";
                case "uniqueidentifier":
                    return "Guid" + nullableSuffix;
                case "text":
                default:
                    return "string";
                //default:
                //    throw new ArgumentException("Unsupported type");
            }
        }
    }
}
