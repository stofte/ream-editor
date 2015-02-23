using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.Installer;
using LinqEditor.Core.CodeAnalysis.Compiler;
using LinqEditor.Core.CodeAnalysis.Services;
using LinqEditor.Core.Helpers;
using LinqEditor.Core.Models.Database;
using LinqEditor.Core.Schema.Services;
using LinqEditor.Core.Settings;
using LinqEditor.Core.Templates;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LinqEditor.Core.Backend.Tests
{
    [TestFixture(Category="Database")]
    public class BackgroundSessionFactoryDatabaseTests
    {
        LinqEditor.Test.Common.SqlServer.Database _database;
        string _schemaAssemblyPath;
        string _query1AssemblyPath;
        byte[] _query2AssemblyBytes;
        byte[] _query3AssemblyBytes;
        DatabaseSchema _schemaModel;
        Guid _schemaId = Guid.NewGuid();
        Guid _queryId1 = Guid.NewGuid();
        Guid _queryId2 = Guid.NewGuid();
        Guid _queryId3 = Guid.NewGuid();
        Guid _connectionId;
        IConnectionStore _testConnections;

        [TestFixtureSetUp]
        public void Initialize()
        {
            _database = new LinqEditor.Test.Common.SqlServer.Database("BackgroundSessionFactoryDatabaseTests");
            var schemaProvider = new SqlSchemaProvider();
            var templateService = new TemplateService();
            _schemaModel = schemaProvider.GetSchema(_database.ConnectionString);
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

            _connectionId = Guid.NewGuid();
            var mockConnections = new Mock<IConnectionStore>();
            mockConnections.Setup(x => x.Connections).Returns(new Connection[]
            {
                new Connection 
                { 
                    ConnectionString = _database.ConnectionString, 
                    Id = _connectionId, 
                    CachedSchemaFileName = _schemaAssemblyPath, 
                    CachedSchemaNamespace = _schemaId.ToIdentifierWithPrefix(SchemaConstants.SchemaPrefix)
                }
            }.AsEnumerable());
            _testConnections = mockConnections.Object;
        }

        [Test]
        public async void Can_Initialize_Second_BackgroundSession_After_Initialization()
        {
            
            var container = new WindsorContainer();
            container.AddFacility<TypedFactoryFacility>();

            container.Install(FromAssembly.Containing<IConnectionStore>()); // core
            container.Install(FromAssembly.Containing<ITemplateService>()); // core.templates
            container.Install(FromAssembly.Containing<IAsyncSessionFactory>()); // core.backend
            container.Install(FromAssembly.Containing<ISqlSchemaProvider>()); // core.schema
            container.Install(FromAssembly.Containing<ITemplateCodeAnalysis>()); // core.codeanalysis

            container.Register(Component.For<IConnectionStore>()
                                        .Instance(_testConnections)
                                        .Named("test-connections")
                                        .IsDefault());

            var factory1 = container.Resolve<IAsyncSessionFactory>();
            var factory2 = container.Resolve<IAsyncSessionFactory>();
            var id = Guid.NewGuid();

            int firstHash = 0;
            int secondHash = 0;

            Task startSession = Task.Run(() => 
            {
                var s = factory1.Create(id);
                firstHash = s.GetHashCode();
                s.InitializeAsync(_connectionId);
                s.LoadAppDomainAsync();
            });

            // first session loads and inits session
            Task.WaitAll(startSession);

            Task loadSecond = Task.Run(() => 
            {
                var s = factory2.Create(id);
                secondHash = s.GetHashCode();
            });

            Task.WaitAll(loadSecond);

            // second session must be same instance
            Assert.AreEqual(firstHash, secondHash);
        }
    }
}
