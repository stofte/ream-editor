using LinqEditor.Core.Schema.Models;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace LinqEditor.Core.Schema.Services
{
    public class SqlSchemaProvider : ISqlSchemaProvider
    {
        public DatabaseSchema GetSchema(string connectionString)
        {
            var tables = new List<TableSchema>();

            using (var conn = new SqlConnection(connectionString))
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
                Tables = tables
            };
        }

        private string SqlTypeToDotNetType(string type, string nullable)
        {
            var isNullable = nullable == "YES";
            switch (type)
            {
                case "int":
                    return isNullable ? "int?" : "int";
                case "nvarchar":
                case "text":
                default:
                    return "string";
            }
        }
    }
}
