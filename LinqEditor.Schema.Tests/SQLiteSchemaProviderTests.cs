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
    [TestFixture]
    public class SQLiteSchemaProviderTests
    {
        SQLiteTestDb _database;

        [TestFixtureSetUp]
        public void Init()
        {
            _database = new SQLiteTestDb("test.sqlite");
        }

        [TestFixtureTearDown]
        public void Cleanup()
        {
            _database.Dispose();
        }

        [Test]
        public async void Can_Load_Schema_For_Simple_Schema()
        {
            var provider = new SQLiteSchemaProvider();

        }
    }
}
