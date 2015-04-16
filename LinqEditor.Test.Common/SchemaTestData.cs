using LinqEditor.Core.Models.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Test.Common
{
    public class SchemaTestData
    {
        public static DatabaseSchema Schema1 = new DatabaseSchema
        {
            Name = "sqlite",
            Tables = new List<TableSchema> 
                { 
                    new TableSchema 
                    { 
                        Name = "Foo", 
                        Columns = new List<ColumnSchema> 
                        { 
                            new ColumnSchema {Index = 0, Name = "Id", Type = typeof(int)},
                            new ColumnSchema {Index = 1, Name = "Text", Type = typeof(string)},
                        } 
                    } 
                }
        };

        public static ServerSchema Schema2 = new ServerSchema
        {
            Databases = new List<DatabaseSchema> 
                {
                    new DatabaseSchema
                    {
                        Name = "sqlserver",
                        Tables = new List<TableSchema> 
                        { 
                            new TableSchema 
                            { 
                                Name = "Bar", 
                                Columns = new List<ColumnSchema> 
                                { 
                                    new ColumnSchema {Index = 0, Name = "Id", Type = typeof(int)},
                                    new ColumnSchema {Index = 1, Name = "Value", Type = typeof(decimal)},
                                } 
                            } 
                        }
                    }
                }
        };

        public static DatabaseSchema Schema3 = new DatabaseSchema
        {
            Name = "sqlserver2",
            Tables = new List<TableSchema> 
                { 
                    new TableSchema 
                    { 
                        Name = "Baz", 
                        Columns = new List<ColumnSchema> 
                        { 
                            new ColumnSchema {Index = 0, Name = "Id", Type = typeof(int)},
                            new ColumnSchema {Index = 1, Name = "Test", Type = typeof(string)},
                        } 
                    } 
                }
        };
    }
}
