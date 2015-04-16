using LinqEditor.Core;
using LinqEditor.Core.Helpers;
using LinqEditor.Core.Models;
using LinqEditor.Core.Models.Database;
using LinqEditor.Core.Settings;
using LinqEditor.Schema;
using LinqEditor.Test.Common;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.DataAccess.Tests
{
    [TestFixture]
    public class AssemblyFileRepositoryTests
    {
        Connection _conn1;
        Connection _conn2;
        Connection _conn3;

        ISchemaProvider _schemaProvider;
        IConnectionStore _connectionStore;

        DatabaseSchema _schema1 = SchemaTestData.Schema1;
        DatabaseSchema _schema2 = SchemaTestData.Schema3;
        ServerSchema _schema3 = SchemaTestData.Schema2;

        [TestFixtureSetUp]
        public void Setup()
        {
            FileSystem.Mode(false);
            _conn1 = new SqlServerConnection
            {
                Id = Guid.NewGuid(),
                ConnectionString = SqlServerTestData.Connstr1
            };

            _conn2 = new SqlServerConnection
            {
                Id = Guid.NewGuid(),
                ConnectionString = SqlServerTestData.Connstr2
            };

            _conn3 = new SqlServerConnection
            {
                Id = Guid.NewGuid(),
                ConnectionString = SqlServerTestData.Connstr4_No_Catalog
            };

            var schemaProviderMock = new Mock<ISchemaProvider>();
            var connectionStoreMock = new Mock<IConnectionStore>();

            schemaProviderMock.Setup(x => x.GetDatabaseSchema(It.Is<SqlServerConnection>(c => c == _conn1)))
                .Returns(() => Task.FromResult(_schema1));

            _schemaProvider = schemaProviderMock.Object;
            _connectionStore = connectionStoreMock.Object;
        }

        [TestFixtureTearDown]
        public void Cleanup()
        {
            FileSystem.Mode(false);
        }

        [Test]
        public void Constructor_Requires_Non_Null_Arguments()
        {
            Assert.Throws<ArgumentNullException>(() => new AssemblyFileRepository(null, _connectionStore));
            Assert.Throws<ArgumentNullException>(() => new AssemblyFileRepository(null, null));
            Assert.Throws<ArgumentNullException>(() => new AssemblyFileRepository(_schemaProvider, null));
        }

        [Test]
        public async void Returns_Non_Null_File_Path()
        {
            var provider = new AssemblyFileRepository(_schemaProvider, _connectionStore);
            string path = await provider.GetAssemblyFilePath(_conn1);
            Assert.IsNotNull(path);
        }

        [Test]
        public void Passing_Null_Connection_Throws()
        {
            var provider = new AssemblyFileRepository(_schemaProvider, _connectionStore);
            Assert.Throws<ArgumentNullException>(async () => await provider.GetAssemblyFilePath(null));
        }

        [Test]
        public void Passing_Empty_Connection_Throws()
        {
            var provider = new AssemblyFileRepository(_schemaProvider, _connectionStore);
            var conn = new SqlServerConnection();
            Assert.Throws<ArgumentException>(async () => await provider.GetAssemblyFilePath(conn));
        }

        [Test]
        public async void After_Returning_ConnectionStore_Has_Received_Hash_Value_Of_Schema()
        {
            var connMock = new Mock<IConnectionStore>();
            var schemaMock = new Mock<ISchemaProvider>();

            schemaMock.Setup(x => x.GetDatabaseSchema(It.Is<SqlServerConnection>(c => c == _conn1)))
                .Returns(() => Task.FromResult(_schema1));
            schemaMock.Setup(x => x.GetDatabaseSchema(It.Is<SqlServerConnection>(c => c == _conn2)))
                .Returns(() => Task.FromResult(_schema2));
            schemaMock.Setup(x => x.GetServerSchema(It.Is<SqlServerConnection>(c => c == _conn3)))
                .Returns(() => Task.FromResult(_schema3));

            var hash1 = SerializationHelper.Hash(_schema1);
            var hash2 = SerializationHelper.Hash(_schema2);
            var hash3 = SerializationHelper.Hash(_schema3);
            
            var provider = new AssemblyFileRepository(schemaMock.Object, connMock.Object);

            await provider.GetAssemblyFilePath(_conn1);
            await provider.GetAssemblyFilePath(_conn2);
            await provider.GetAssemblyFilePath(_conn3);

            // check that we passed the expected hash to the connection store
            connMock.Verify(x => x.Update(It.Is<Connection>(c => c.SchemaHash == hash1)), Times.AtLeastOnce);
            connMock.Verify(x => x.Update(It.Is<Connection>(c => c.SchemaHash == hash2)), Times.AtLeastOnce);
            connMock.Verify(x => x.Update(It.Is<Connection>(c => c.SchemaHash == hash3)), Times.AtLeastOnce);
        }
    }
}
