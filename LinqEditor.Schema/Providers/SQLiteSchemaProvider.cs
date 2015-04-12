using LinqEditor.Core.Models;
using LinqEditor.Core.Models.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace LinqEditor.Schema.Providers
{
    public class SQLiteSchemaProvider : ISQLiteSchemaProvider
    {
        public async Task<DatabaseSchema> GetDatabaseSchema(SQLiteFileConnection connection)
        {
            var tables = new List<TableSchema>();
            string dbName = string.Empty;

            using (var conn = new SQLiteConnection(connection.ConnectionString))
            {
                var tblCmd = new SQLiteCommand("select * from sqlite_master where type = 'table'", conn);
                await conn.OpenAsync();
                var reader = await tblCmd.ExecuteReaderAsync();

                while (reader.Read())
                {
                    var colList = new List<ColumnSchema>();
                    var tblName = reader["tbl_name"].ToString();

                    var colCmd = new SQLiteCommand(string.Format("pragma table_info({0})", tblName), conn);
                    var colReader = await colCmd.ExecuteReaderAsync();
                    
                    while (colReader.Read())
                    {
                        var colIndex = (int)(long)colReader["cid"];
                        var colName = colReader["name"].ToString();
                        var colType = colReader["type"].ToString().ToLower();
                        var colNullable = ((long)colReader["notnull"]) != 0;
                        var dotnetType = MapSqlTypeToDotNetType(colType, colNullable);
                        colList.Add(new ColumnSchema
                        {
                            Index = colIndex,
                            Name = colName,
                            Type = dotnetType
                        });
                    }

                    tables.Add(new TableSchema
                    {
                        Columns = colList,
                        Name = tblName,
                        Catalog = connection.DatabaseName // own convention
                    });
                }
            }

            return new DatabaseSchema
            {
                Name = connection.DatabaseName,
                Tables = tables
            };
        }

        private Type MapSqlTypeToDotNetType(string type, bool isNullable)
        {
            switch (type)
            {
                case "int":
                    return isNullable ? typeof(int?) : typeof(int);
                case "text":
                default:
                    return typeof(string);
            }
        }
    }
}
