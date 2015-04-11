using LinqEditor.Core.Models;
using LinqEditor.Core.Models.Database;
using LinqEditor.Core.Settings;
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
    public class AssemblyFileProviderTests
    {
        Connection _sqlServerConn;
        IConnectionStore _connectionStore;

        [TestFixtureSetUp]
        public void Setup()
        {
            _sqlServerConn = new SqlServerConnection
            {
                Id = Guid.NewGuid(),
                ConnectionString = DatabaseTestData.Connstr1
            };

            var connStore = new Mock<IConnectionStore>();
            connStore.Setup(x => x.Connections).Returns(new Connection[] { _sqlServerConn });
            _connectionStore = connStore.Object;
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Passing_Null_ConnectionStore_Throws()
        {
            new AssemblyFileProvider(null);
        }

        [Test]
        public async void Returns_Non_Null_File_Path()
        {
            var provider = new AssemblyFileProvider(_connectionStore);
            string path = await provider.GetSchemaPath(_sqlServerConn);
            Assert.IsNotNull(path);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public async void Passing_Null_Connection_Throws()
        {
            var provider = new AssemblyFileProvider(_connectionStore);
            string path = await provider.GetSchemaPath(null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public async void Passing_Empty_Connection_Throws()
        {
            var provider = new AssemblyFileProvider(_connectionStore);
            var conn = new SqlServerConnection();
            string path = await provider.GetSchemaPath(conn);
        }
    }
}
