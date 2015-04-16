using LinqEditor.Core.Models;
using LinqEditor.Core.Models.Database;
using System;
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
                    return isNullable ? typeof(DateTime?) : typeof(DateTime);
                case "datetimeoffset":
                    return isNullable ? typeof(DateTimeOffset?) : typeof(DateTimeOffset);
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
