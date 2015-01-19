using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LinqEditor.Backend;
using System.Windows.Forms;

namespace LinqEditor.Generator.Test.SqlServer
{
    [TestClass]
    public class SchemaTests
    {
        static TestDatabase database;

        #region Database SQL
        static string DbSchema = @"
            create table Foo (
                Id int PRIMARY KEY,
                Description Text
            );";
        static string DbScript = @"
            delete Foo;
            insert into Foo(Id,Description) values(0, 'Foo 0');
            insert into Foo(Id,Description) values(1, 'Foo 1');
            insert into Foo(Id,Description) values(2, 'Foo 2');
            insert into Foo(Id,Description) values(3, 'Foo 3');";
        #endregion


        [ClassInitialize]
        public static void Setup(TestContext context)
        {
            
            database = new TestDatabase("UnitTest", DbSchema, DbScript);
        }

        [TestMethod]
        public void Can_Execute_Query()
        {
            //var comp = new CSharpCodeService();
            
            //var generator = new LinqEditor.Generator.Query.SqlServer();
            //generator.ConnectionString = database.ConnectionString;
            //generator.SourceCode = "Foo.Dump();";
            //var src = generator.TransformText();
            //var assembly = comp.Run(src, generator.SourceCode);
            //var programType = assembly.GetType(string.Format("{0}.Program", generator.GeneratedNamespace));
            //var instance = Activator.CreateInstance(programType) as IQueryUnit;
            //instance.Execute();
            
            //var dt = instance.Result.Get().FirstOrDefault();
        }

        [TestInitialize]
        public void TestInitialize()
        {
            database.RecreateTestData();
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            database.Dispose();
        }
    }
}