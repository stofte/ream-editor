using LinqEditor.Common.Tests;
using LinqEditor.Core.Backend.Isolated;
using LinqEditor.Core.CodeAnalysis.Compiler;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.Backend.Tests
{
    [TestFixture(Category="SqlServerIntegration")]
    public class RunnerTests
    {
        [Test]
        public void Can_Load_Initial_Assembly()
        {
            var container = new Isolated<Runner>();
            var initResult = container.Value.Initialize(_initialAssemblyPath);
            container.Dispose();
            Assert.IsNull(initResult.Error);
        }

        SqlServerTestDatabase _database;
        string _initialAssemblyPath;

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

        [TestFixtureSetUp]
        public void Initialize()
        {
            _database = new SqlServerTestDatabase("UnitTest", _dbSchema, _dbScript);
            var compiler = new CSharpCompiler();
            var result = compiler.Compile(_initializeSource, "Initial", Path.GetTempPath());
            _initialAssemblyPath = result.AssemblyPath;
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
            if (File.Exists(_initialAssemblyPath))
            {
                File.Delete(_initialAssemblyPath);
            }
        }
    }
}
