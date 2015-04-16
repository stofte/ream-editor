using LinqEditor.Core.Models;
using LinqEditor.Core.Models.Database;
using LinqEditor.Test.Common;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Schema.Tests
{
    [TestFixture]
    public class SchemaProviderTests
    {
        ISQLiteSchemaProvider _sqliteProvider;
        ISqlServerSchemaProvider _sqlServerProvider;
        SqlServerConnection _sqlServerConnection;
        SQLiteFileConnection _sqliteConnection;

        DatabaseSchema _schema1 = SchemaTestData.Schema1;
        ServerSchema _schema2 = SchemaTestData.Schema2;
        DatabaseSchema _schema3 = SchemaTestData.Schema3;

        [TestFixtureSetUp]
        public void Initialize()
        {
            _sqlServerConnection = new SqlServerConnection();
            _sqliteConnection = new SQLiteFileConnection();

            var sqliteMock = new Mock<ISQLiteSchemaProvider>();
            var sqlserverMock = new Mock<ISqlServerSchemaProvider>();

            sqliteMock.Setup(x => x.GetDatabaseSchema(It.IsAny<SQLiteFileConnection>())).Returns(() => Task.FromResult(_schema1));
            sqlserverMock.Setup(x => x.GetServerSchema(It.IsAny<SqlServerConnection>())).Returns(() => Task.FromResult(_schema2));
            sqlserverMock.Setup(x => x.GetDatabaseSchema(It.IsAny<SqlServerConnection>())).Returns(() => Task.FromResult(_schema3));

            _sqliteProvider = sqliteMock.Object;
            _sqlServerProvider = sqlserverMock.Object;
        }

        [Test]
        public void Requires_Underlying_Schema_Providers()
        {
            Assert.Throws<ArgumentNullException>(() => new SchemaProvider(null, null));
            Assert.Throws<ArgumentNullException>(() => new SchemaProvider(null, _sqlServerProvider));
            Assert.Throws<ArgumentNullException>(() => new SchemaProvider(_sqliteProvider, null));
        }

        [Test]
        public async void GetDatabaseSchema_Returns_Proper_Schema_For_SqlServer_Connection()
        {
            var provider = new SchemaProvider(_sqliteProvider, _sqlServerProvider);
            var schema = await provider.GetDatabaseSchema(_sqlServerConnection);
            Assert.AreSame(_schema3, schema);
        }

        [Test]
        public async void GetServerSchema_Returns_Proper_Schema_For_SqlServer_Connection()
        {
            var provider = new SchemaProvider(_sqliteProvider, _sqlServerProvider);
            var schema = await provider.GetServerSchema(_sqlServerConnection);
            Assert.AreSame(_schema2, schema);
        }

        [Test]
        public async void GetDatabaseSchema_Returns_Proper_Schema_For_SQLite_Connection()
        {
            var provider = new SchemaProvider(_sqliteProvider, _sqlServerProvider);
            var schema = await provider.GetDatabaseSchema(_sqliteConnection);
            Assert.AreSame(_schema1, schema);
        }
    }
}
