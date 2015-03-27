using LinqEditor.Core.Models.Database;
using LinqEditor.Core.Schema.Services;
using LinqEditor.Core.Templates;
using LinqEditor.Core.Helpers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqEditor.Core.CodeAnalysis.Compiler;
using System.IO;
using LinqEditor.Core.Containers;
using LinqEditor.Core.Settings;

namespace LinqEditor.Core.Tests
{
    [TestFixture(Category = "Database")]
    public class DatabaseContainerTests
    {
        LinqEditor.Test.Common.MSSQLServer.MSSQLServerTestDb _database;
        string _schemaAssemblyPath;
        string _query1AssemblyPath;
        byte[] _query2AssemblyBytes;
        byte[] _query3AssemblyBytes;
        DatabaseSchema _schemaModel;
        Guid _schemaId = Guid.NewGuid();
        Guid _queryId1 = Guid.NewGuid();
        Guid _queryId2 = Guid.NewGuid();
        Guid _queryId3 = Guid.NewGuid();

        [TestFixtureSetUp]
        public void Initialize()
        {
            _database = new LinqEditor.Test.Common.MSSQLServer.MSSQLServerTestDb("DatabaseContainerTests");
            var schemaProvider = new MSSQLServerSchemaProvider();
            var templateService = new TemplateService();
            var conn = new Connection { Id = Guid.NewGuid(), ConnectionString = _database.ConnectionString, Kind = Models.Editor.ProgramType.MSSQLServer };
            _schemaModel = schemaProvider.GetSchema(conn);
            var schemaSource = templateService.GenerateSchema(_schemaId, _schemaModel);
            var schemaResult = CSharpCompiler.CompileToFile(schemaSource, _schemaId.ToIdentifierWithPrefix(SchemaConstants.SchemaPrefix), PathUtility.TempPath);
            _schemaAssemblyPath = schemaResult.AssemblyPath;
            var querySource1 = templateService.GenerateQuery(_queryId1, "Foo.Dump();", _schemaId.ToIdentifierWithPrefix(SchemaConstants.SchemaPrefix));
            var querySource2 = templateService.GenerateQuery(_queryId2, "Foo.Dump();", _schemaId.ToIdentifierWithPrefix(SchemaConstants.SchemaPrefix));
            var querySource3 = templateService.GenerateQuery(_queryId3, "TypeTestTable.Take(1).Dump();", _schemaId.ToIdentifierWithPrefix(SchemaConstants.SchemaPrefix));

            var file1Result = CSharpCompiler.CompileToFile(querySource1, _queryId1.ToIdentifierWithPrefix(SchemaConstants.QueryPrefix), PathUtility.TempPath, _schemaAssemblyPath);
            _query1AssemblyPath = file1Result.AssemblyPath;
            var bytes2Result = CSharpCompiler.CompileToBytes(querySource2, _queryId2.ToIdentifierWithPrefix(SchemaConstants.QueryPrefix), _schemaAssemblyPath);
            _query2AssemblyBytes = bytes2Result.AssemblyBytes;
            var bytes3Result = CSharpCompiler.CompileToBytes(querySource3, _queryId3.ToIdentifierWithPrefix(SchemaConstants.QueryPrefix), _schemaAssemblyPath);
            _query3AssemblyBytes = bytes3Result.AssemblyBytes;
        }

        [SetUp]
        public void Setup()
        {
            _database.RecreateTestData();
        }

        [TestFixtureTearDown]
        public void Cleanup()
        {
            _database.Dispose();

            if (File.Exists(_schemaAssemblyPath))
            {
                File.Delete(_schemaAssemblyPath);
            }
            if (File.Exists(_query1AssemblyPath))
            {
                File.Delete(_query1AssemblyPath);
            }
        }

        [Test]
        public void Can_Load_Initial_Schema_Assembly()
        {
            var container = new IsolatedDatabaseContainer();
            var initResult = container.Value.Initialize(_schemaAssemblyPath);
            container.Dispose();
            Assert.IsNull(initResult.Error);
        }

        [Test]
        public void Can_Execute_Query_Assembly_And_Fetch_Database_Rows_With_Basic_Types_Only_Using_Byte_Assembly()
        {
            var container = new IsolatedDatabaseContainer();
            var initResult = container.Value.Initialize(_schemaAssemblyPath);
            var executeResult = container.Value.Execute(_query2AssemblyBytes);

            container.Dispose();
            var rows = executeResult.Tables.First();
            Assert.IsNull(executeResult.Exception);
            Assert.AreEqual(4, executeResult.Tables.First().Rows.Count);
            Assert.AreEqual("Foo 3", executeResult.Tables.First().Rows[3][1]);
        }

        [Test]
        public void Can_Execute_Query_Assembly_And_Fetch_Database_Rows_With_Basic_Types_Only_Using_File_Assembly()
        {
            var container = new IsolatedDatabaseContainer();
            var initResult = container.Value.Initialize(_schemaAssemblyPath);
            var executeResult = container.Value.Execute(_query1AssemblyPath);

            container.Dispose();
            var rows = executeResult.Tables.First();
            Assert.IsNull(executeResult.Exception);
            Assert.AreEqual(4, executeResult.Tables.First().Rows.Count);
            Assert.AreEqual("Foo 3", executeResult.Tables.First().Rows[3][1]);
        }

        [Test]
        public void Can_Execute_Query_Assembly_And_Fetch_Database_Rows_With_All_DataTypes()
        {
            var container = new IsolatedDatabaseContainer();
            var initResult = container.Value.Initialize(_schemaAssemblyPath);
            var executeResult = container.Value.Execute(_query3AssemblyBytes);

            container.Dispose();
            var rows = executeResult.Tables.First();
            Assert.IsNull(executeResult.Exception);
            Assert.AreEqual(1, executeResult.Tables.First().Rows.Count);
            Assert.AreEqual(2147483647, (int)executeResult.Tables.First().Rows[0]["intcol"]);
        }
    }
}
