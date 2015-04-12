using LinqEditor.Core.Models;
using LinqEditor.Schema.Providers;
using LinqEditor.Test.Common.SQLite;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Schema.Tests
{
    [TestFixture(Category="FileSystem")]
    public class SQLiteSchemaProviderTests
    {
        SQLiteTestDb _database;
        SQLiteFileConnection _connection;

        [TestFixtureSetUp]
        public void Init()
        {
            _database = new SQLiteTestDb("test.sqlite");
            _connection = new SQLiteFileConnection
            {
                ConnectionString = _database.ConnectionString
            };
        }

        [TestFixtureTearDown]
        public void Cleanup()
        {
            _database.Dispose();
        }

        [Test]
        public async void Can_Load_Simple_Schema()
        {
            var provider = new SQLiteSchemaProvider();
            var schema = await provider.GetDatabaseSchema(_connection);
            
            Assert.AreEqual("test.sqlite", schema.Name);
            var tblSchema = schema.Tables.Single();
            Assert.AreEqual("test.sqlite", tblSchema.Catalog);
            Assert.AreEqual(2, tblSchema.Columns.Count());
            Assert.AreEqual(typeof(int), tblSchema.Columns.Single(x => x.Name == "Id").Type);
            Assert.AreEqual(typeof(string), tblSchema.Columns.Single(x => x.Name == "Description").Type);
        }
    }
}
