using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.Installer;
using LinqEditor.Core.CodeAnalysis.Compiler;
using LinqEditor.Core.CodeAnalysis.Services;
using LinqEditor.Core.Helpers;
using LinqEditor.Core.Models.Analysis;
using LinqEditor.Core.Models.Database;
using LinqEditor.Core.Schema.Services;
using LinqEditor.Core.Settings;
using LinqEditor.Core.Templates;
using LinqEditor.Test.Common;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LinqEditor.Core.Session.Tests
{
    [TestFixture(Category="Database")]
    public class AsyncSessionFactoryDatabaseTests
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
        Guid _connectionId;
        IConnectionStore _testConnections;
        IWindsorContainer _container;

        [TestFixtureSetUp]
        public void Initialize()
        {
            _database = new LinqEditor.Test.Common.SqlServer.SqlServerTestDb("AsyncSessionFactoryDatabaseTests");
            var schemaProvider = new SqlSchemaProvider();
            var templateService = new TemplateService();
            _schemaModel = schemaProvider.GetSchema(_database.ConnectionString);
            var schemaSource = templateService.GenerateSchema(_schemaId, _schemaModel);
            var schemaResult = CSharpCompiler.CompileToFile(schemaSource, _schemaId.ToIdentifierWithPrefix(SchemaConstants.SchemaPrefix), PathUtility.TempPath);
            _schemaAssemblyPath = schemaResult.AssemblyPath;
            var querySource1 = templateService.GenerateQuery(_queryId1, "Foo.Dump();", _schemaId.ToIdentifierWithPrefix(SchemaConstants.SchemaPrefix));
            var querySource2 = templateService.GenerateQuery(_queryId2, "Foo.Dump();", _schemaId.ToIdentifierWithPrefix(SchemaConstants.SchemaPrefix));
            var querySource3 = templateService.GenerateQuery(_queryId3, "TypeTestTable.Take(1).Dump();", _schemaId.ToIdentifierWithPrefix(SchemaConstants.SchemaPrefix));
            var querySource4 = templateService.GenerateQuery(_queryId3, "TypeTestTable.Take(1)", _schemaId.ToIdentifierWithPrefix(SchemaConstants.SchemaPrefix));

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

            _container = new WindsorContainer();
            _container.AddFacility<TypedFactoryFacility>();

            _container.Install(FromAssembly.Containing<IConnectionStore>()); // core
            _container.Install(FromAssembly.Containing<ITemplateService>()); // core.templates
            _container.Install(FromAssembly.Containing<IAsyncSessionFactory>()); // core.backend
            _container.Install(FromAssembly.Containing<ISqlSchemaProvider>()); // core.schema
            _container.Install(FromAssembly.Containing<ITemplateCodeAnalysis>()); // core.codeanalysis

            _container.Register(Component.For<IConnectionStore>()
                                        .Instance(_testConnections)
                                        .Named("test-connections")
                                        .IsDefault());
        }


        [SetUp]
        public void Setup()
        {
            _database.RecreateTestData();
        }

        [Test]
        public void Can_Initialize_Second_BackgroundSession_After_Initialization()
        {
            var factory1 = _container.Resolve<IAsyncSessionFactory>();
            var factory2 = _container.Resolve<IAsyncSessionFactory>();
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

        [TestCase(VSCompletionTestData.ThisAccessInDb)]
        [TestCase(VSCompletionTestData.FooColumnAccessInDb)]
        public async void Maps_Member_Completions_To_Database_Types(string testDataKey)
        {
            var stub = VSCompletionTestData.SourceAndData[testDataKey].Item1;
            var offset = VSCompletionTestData.SourceAndData[testDataKey].Item2;
            var vsEntries = VSCompletionTestData.SourceAndData[testDataKey].Item3.ToArray();
            var factory = _container.Resolve<IAsyncSessionFactory>();
            var id = Guid.NewGuid();
            var session = factory.Create(id);
            await session.InitializeAsync(_connectionId);
            await session.LoadAppDomainAsync();
            var result = await session.AnalyzeAsync(stub, offset);
            var suggestions = result.MemberCompletions.ToArray();

            for (var i = 0; i < vsEntries.Length; i++)
            {
                Assert.AreEqual(vsEntries[i].Item1, suggestions[i].Value);
                Assert.AreEqual(vsEntries[i].Item2, suggestions[i].Kind);
            }
        }

        [Test]
        public async void Auto_Completes_Missing_Source_Code_Elements()
        {
            var factory = _container.Resolve<IAsyncSessionFactory>();
            var id = Guid.NewGuid();
            var session = factory.Create(id);
            await session.InitializeAsync(_connectionId);
            await session.LoadAppDomainAsync();
            var result = await session.ExecuteAsync("Foo");
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.Tables.Count());
            Assert.AreEqual(4, result.Tables.First().Rows.Count);
        }

        [TestCase("Foo.Dump()")]
        [TestCase("Foo.   Dump( )  ")]
        [TestCase("Foo. Dump  (   ) ")]
        [TestCase(@"Foo. 
Dump
(   
                                    ) ")]
        public async void Detects_Dump_Method_When_Auto_Completing_Source(string query)
        {
            var factory = _container.Resolve<IAsyncSessionFactory>();
            var id = Guid.NewGuid();
            var session = factory.Create(id);
            await session.InitializeAsync(_connectionId);
            await session.LoadAppDomainAsync();
            var result = await session.ExecuteAsync(query);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.Tables.Count());
            Assert.AreEqual(4, result.Tables.First().Rows.Count);
        }
    }
}
