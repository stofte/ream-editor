using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using LinqEditor.Core.CodeAnalysis.Compiler;
using LinqEditor.Core.Containers;
using LinqEditor.Core.Models.Database;
using LinqEditor.Core.Schema.Services;
using LinqEditor.Core.Scopes;
using LinqEditor.Core.Templates;
using LinqEditor.Core.Helpers;
using NUnit.Framework;
using System;
using Castle.Windsor.Installer;
using LinqEditor.Core.Settings;

namespace LinqEditor.Core.Tests
{
    [TestFixture(Category="Database")]
    public class WindsorContainerDatabaseTests
    {
        LinqEditor.Test.Common.SqlServer.SqlServerTestDb _database;
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
            _database = new LinqEditor.Test.Common.SqlServer.SqlServerTestDb("WindsorContainerDatabaseTests");
            var schemaProvider = new SqlServerSchemaProvider();
            var templateService = new TemplateService();
            var conn = new Connection { Id = Guid.NewGuid(), ConnectionString = _database.ConnectionString, Kind = Models.Editor.ProgramType.SqlServer };
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

        [Test]
        public void Can_Load_Different_DatabaseContainer_Instances()
        {
            var container = new WindsorContainer();
            container.AddFacility<TypedFactoryFacility>();
            container.Install(FromAssembly.Containing<IConnectionStore>()); // core
            var containerFactory = container.Resolve<IIsolatedDatabaseContainerFactory>();
  
            var instance1 = containerFactory.Create();
            var instance2 = containerFactory.Create();
            Assert.AreNotSame(instance1, instance2);
        }
    }
}
