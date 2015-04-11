using LinqEditor.Core.Models;
using LinqEditor.Core.Models.Editor;
using LinqEditor.Core.Settings;
using LinqEditor.Schema.Providers;
using LinqEditor.Test.Common.SqlServer;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Schema.Tests
{
    [TestFixture(Category="Database")]
    public class SqlServerSchemaProviderTests
    {
        SqlServerTestDb _database;
        Connection _connection;

        [TestFixtureSetUp]
        public void Initialize()
        {
            _database = new SqlServerTestDb("SqlServerSchemaProviderTests", Scripts.Schema, string.Empty);
            _connection = new SqlServerConnection
            {
                ConnectionString = _database.ConnectionString,
                Id = Guid.NewGuid()
            };
        }

        [TestFixtureTearDown]
        public void Cleanup()
        {
            _database.Dispose();
        }

        [SetUp]
        public void Setup()
        {
            _database.RecreateTestData();
        }

        [Test]
        public void Throws_If_Null_Connection_Is_Passed()
        {
            var provider = new SqlServerSchemaProvider();
            Assert.Throws<ArgumentNullException>(async () =>
            {
                await provider.GetServerSchema(null);
            });
        }

        [Test]
        public async void Connection_String_With_Database_Returns_Single_Database()
        {
            var provider = new SqlServerSchemaProvider();
            var schema = await provider.GetServerSchema(_connection);

            Assert.AreEqual(2, schema.Databases.Single().Tables.Count());
        }

        [Test]
        public async void Maps_Sql_Types_To_Expected_DotNet_Types()
        {
            var provider = new SqlServerSchemaProvider();

            var schema = await provider.GetServerSchema(_connection);
            var tbl = schema.Databases.Single().Tables.Single(x => x.Name == "TypeTestTable");
            
            Assert.AreEqual(typeof(long?), tbl.Columns.Single(x => x.Name == "bigintcol").Type);
            Assert.AreEqual(typeof(bool?), tbl.Columns.Single(x => x.Name == "bitcol").Type);
            Assert.AreEqual(typeof(decimal?), tbl.Columns.Single(x => x.Name == "decimalcol").Type);
            Assert.AreEqual(typeof(int?), tbl.Columns.Single(x => x.Name == "intcol").Type);
            Assert.AreEqual(typeof(decimal?), tbl.Columns.Single(x => x.Name == "moneycol").Type);
            Assert.AreEqual(typeof(long?), tbl.Columns.Single(x => x.Name == "numericcol").Type);
            Assert.AreEqual(typeof(int?), tbl.Columns.Single(x => x.Name == "smallintcol").Type);
            Assert.AreEqual(typeof(decimal?), tbl.Columns.Single(x => x.Name == "smallmoneycol").Type);
            Assert.AreEqual(typeof(int?), tbl.Columns.Single(x => x.Name == "tinyintcol").Type);
            Assert.AreEqual(typeof(double?), tbl.Columns.Single(x => x.Name == "floatcol").Type);
            Assert.AreEqual(typeof(double?), tbl.Columns.Single(x => x.Name == "realcol").Type);
            Assert.AreEqual(typeof(DateTime?), tbl.Columns.Single(x => x.Name == "datecol").Type);
            Assert.AreEqual(typeof(DateTime?), tbl.Columns.Single(x => x.Name == "datetime2col").Type);
            Assert.AreEqual(typeof(DateTime?), tbl.Columns.Single(x => x.Name == "datetimecol").Type);
            Assert.AreEqual(typeof(DateTime?), tbl.Columns.Single(x => x.Name == "datetimeoffsetcol").Type);
            Assert.AreEqual(typeof(DateTime?), tbl.Columns.Single(x => x.Name == "smalldatetimecol").Type);
            Assert.AreEqual(typeof(TimeSpan?), tbl.Columns.Single(x => x.Name == "timecol").Type);
            Assert.AreEqual(typeof(string), tbl.Columns.Single(x => x.Name == "charcol").Type);
            Assert.AreEqual(typeof(string), tbl.Columns.Single(x => x.Name == "varcharcol").Type);
            Assert.AreEqual(typeof(string), tbl.Columns.Single(x => x.Name == "varcharmaxcol").Type);
            Assert.AreEqual(typeof(string), tbl.Columns.Single(x => x.Name == "textcol").Type);
            Assert.AreEqual(typeof(string), tbl.Columns.Single(x => x.Name == "ncharcol").Type);
            Assert.AreEqual(typeof(string), tbl.Columns.Single(x => x.Name == "nvarcharcol").Type);
            Assert.AreEqual(typeof(string), tbl.Columns.Single(x => x.Name == "ntextcol").Type);
            Assert.AreEqual(typeof(byte[]), tbl.Columns.Single(x => x.Name == "binarycol").Type);
            Assert.AreEqual(typeof(byte[]), tbl.Columns.Single(x => x.Name == "varbinarycol").Type);
            Assert.AreEqual(typeof(byte[]), tbl.Columns.Single(x => x.Name == "varbinarymaxcol").Type);
            Assert.AreEqual(typeof(byte[]), tbl.Columns.Single(x => x.Name == "rowversioncol").Type);
            Assert.AreEqual(typeof(Guid?), tbl.Columns.Single(x => x.Name == "uniqueidentifiercol").Type);
        }
    }
}
