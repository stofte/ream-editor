using LinqEditor.Core.Schema.Helpers;
using LinqEditor.Common.Tests;
using LinqEditor.Core.Backend.Isolated;
using LinqEditor.Core.CodeAnalysis.Compiler;
using LinqEditor.Core.Schema.Models;
using LinqEditor.Core.Schema.Services;
using LinqEditor.Core.Templates;
using NUnit.Framework;
using System;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqEditor.Common;
using System.Data;

namespace LinqEditor.Core.Backend.Tests
{
    [TestFixture(Category="Database")]
    public class RunnerDatabaseTests
    {
        [Test]
        public void Can_Load_Initial_Schema_Assembly()
        {
            var container = new Isolated<Runner>();
            var initResult = container.Value.Initialize(_schemaAssemblyPath, _database.ConnectionString);
            container.Dispose();
            Assert.IsNull(initResult.Error);
        }

        [Test]
        public void Can_Execute_Query_Assembly_And_Fetch_Database_Rows()
        {
            var container = new Isolated<Runner>();
            var initResult = container.Value.Initialize(_schemaAssemblyPath, _database.ConnectionString);
            var executeResult = container.Value.Execute(_queryAssemblyBytes);
            
            container.Dispose();
            var rows = executeResult.Tables.First();
            Assert.IsNull(executeResult.Exception);
            Assert.AreEqual(4, executeResult.Tables.First().Rows.Count);
            Assert.AreEqual("Foo 3", executeResult.Tables.First().Rows[3][1]);
        }

        #region configuration


        SqlServerTestDatabase _database;
        string _schemaAssemblyPath;
        string _queryAssemblyPath;
        byte[] _queryAssemblyBytes;
        DatabaseSchema _schemaModel;
        Guid _schemaId = Guid.NewGuid();
        Guid _queryId1 = Guid.NewGuid();
        Guid _queryId2 = Guid.NewGuid();

        string _dbSchema = @"
                    create table Foo (
                        Id int PRIMARY KEY,
                        Description Text
                    );";
        string _dbScript = @"
                    delete Foo;
                    insert into Foo(Id,Description) values(0, 'Foo 0');
                    insert into Foo(Id,Description) values(1, 'Foo 1');
                    insert into Foo(Id,Description) values(2, 'Foo 2');
                    insert into Foo(Id,Description) values(3, 'Foo 3');";

        string _initializeSource = @"
using System; 
using System.Threading;

namespace Initial {
    public class SlowQuery {
        public int Execute(int ms) {
            Thread.Sleep(ms);
            return ms;
        }
    }
}
";

        string _dynamicSource = @"
using System; 
using Initial;

namespace Generated {
    public class Program {
        public int Query() {
            var query = new SlowQuery();
            
        }
    }
}
";

        [TestFixtureSetUp]
        public void Initialize()
        {
            _database = new SqlServerTestDatabase("UnitTest", _dbSchema, _dbScript);
            var schemaProvider = new SqlSchemaProvider();
            var templateService = new TemplateService();
            _schemaModel = schemaProvider.GetSchema(_database.ConnectionString);
            var schemaSource = templateService.GenerateSchema(_schemaId, _schemaModel);
            var compiler = new CSharpCompiler();
            var schemaResult = compiler.Compile(schemaSource, _schemaId.ToIdentifierWithPrefix("s"), Utility.TempPath());
            _schemaAssemblyPath = schemaResult.AssemblyPath;
            var querySource1 = templateService.GenerateQuery(_queryId1, "Foo.Dump();", _schemaId.ToIdentifierWithPrefix("s"));
            var querySource2 = templateService.GenerateQuery(_queryId2, "Foo.Dump();", _schemaId.ToIdentifierWithPrefix("s"));

            var fileResult = compiler.Compile(querySource1, _queryId1.ToIdentifierWithPrefix("q"), Utility.TempPath(), _schemaAssemblyPath);
            _queryAssemblyPath = fileResult.AssemblyPath;
            var bytesResult = compiler.Compile(querySource2, _queryId2.ToIdentifierWithPrefix("q"), null, _schemaAssemblyPath);
            _queryAssemblyBytes = bytesResult.AssemblyBytes;
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
            if (File.Exists(_queryAssemblyPath))
            {
                File.Delete(_queryAssemblyPath);
            }
        }

        #endregion
    }
}
