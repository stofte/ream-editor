using LinqEditor.Core.Models.Editor;
using LinqEditor.Core.Settings;
using LinqEditor.Schema.Providers;
using LinqEditor.Test.Common.SqlServer;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LinqEditor.Schema.Tests
{
    [TestFixture(Category="Database")]
    public class SqlServerSchemaProviderMultipleDatabaseTests
    {
        SqlServerTestDb _db1;
        SqlServerTestDb _db2;
        Connection _connection;

        string _schema1 = @"
create table Foo (
    Id int PRIMARY KEY,
    Description Text
);
";
        string _script1 = @"
delete Foo;
insert into Foo(Id,Description) values(0, 'Foo 0');
insert into Foo(Id,Description) values(1, 'Foo 1');
insert into Foo(Id,Description) values(2, 'Foo 2');
insert into Foo(Id,Description) values(3, 'Foo 3');
";

        string _schema2 = @"
create table Bar (
    Id int PRIMARY KEY,
    Value date
);
";

        string _script2 = @"
delete Bar;
insert into Bar(Id,Value) values(0, '2001-01-01');
insert into Bar(Id,Value) values(1, '2001-01-02');
insert into Bar(Id,Value) values(2, '2001-01-03');
insert into Bar(Id,Value) values(3, '2001-01-04');
";

        [TestFixtureSetUp]
        public void Initialize()
        {
            _connection = new Connection
            {
                ConnectionString = SqlServerTestDb.LocalDb
            };

            _db1 = new SqlServerTestDb("DB1", _schema1, _script1);
            _db2 = new SqlServerTestDb("DB2", _schema2, _script2);
        }

        [TestFixtureTearDown]
        public void Cleanup()
        {
            _db1.Dispose();
            _db2.Dispose();
        }

        [SetUp]
        public void Setup()
        {
            _db1.RecreateTestData();
            _db2.RecreateTestData();
        }

        [Test]
        public async void Loads_Multiple_Database_Schemas_When_Using_Connection_String_Without_Database()
        {
            var provider = new SqlServerSchemaProvider();

            var schema = await provider.GetServerSchema(_connection);
            var db1 = schema.Databases.Single(x => x.Name == "DB1");
            var db2 = schema.Databases.Single(x => x.Name == "DB2");

            Assert.AreEqual("Foo", db1.Tables.Single().Name);
            Assert.AreEqual("Bar", db2.Tables.Single().Name);

            var fooTbl = db1.Tables.Single().Columns.Single(x => x.Name == "Description");
            var barTbl = db2.Tables.Single().Columns.Single(x => x.Name == "Value");

            Assert.AreEqual(2, db1.Tables.Single().Columns.Count());
            Assert.AreEqual(typeof(int), db1.Tables.Single().Columns.Single(x => x.Name == "Id").Type);
            Assert.AreEqual(typeof(string), db1.Tables.Single().Columns.Single(x => x.Name == "Description").Type);

            Assert.AreEqual(2, db2.Tables.Single().Columns.Count());
            Assert.AreEqual(typeof(int), db2.Tables.Single().Columns.Single(x => x.Name == "Id").Type);
            Assert.AreEqual(typeof(DateTime?), db2.Tables.Single().Columns.Single(x => x.Name == "Value").Type);
        }
    }
}
