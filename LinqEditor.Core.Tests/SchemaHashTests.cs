using LinqEditor.Core.Models;
using LinqEditor.Core.Models.Database;
using LinqEditor.Core.Schema.Services;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.Tests
{
    [TestFixture]
    public class SchemaHashTests
    {
        DatabaseSchema _schema1;
        ServerSchema _schema2;
        DatabaseSchema _schema3;

        [TestFixtureSetUp]
        public void Initialize()
        {
            _schema1 = new DatabaseSchema
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

            _schema2 = new ServerSchema
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

            _schema3 = new DatabaseSchema
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

        [Test]
        public void Schema_Models_Return_Hash_For_Schema()
        {
            
        }
    }
}
