using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Utility.Test
{
    public class TestDatabase : IDisposable
    {
        private const string LocalDbMaster = "Data Source=(LocalDB)\\v11.0;Initial Catalog=master;Integrated Security=True";

        private readonly string _databaseName;
        private readonly string _fileName;
        private readonly string _schema;
        private readonly string _script;
        public readonly string ConnectionString;

        public TestDatabase(string databaseName, string schema, string script)
        {
            _databaseName = databaseName;
            _fileName = Path.GetFullPath(databaseName);
            _schema = schema;
            _script = script;
            ConnectionString = string.Format("Data Source=(LocalDB)\\v11.0;Initial Catalog={0};Integrated Security=True;" +
                "MultipleActiveResultSets=True;AttachDBFilename={1}.mdf", _databaseName, _fileName);
            CreateDatabase();
        }

        public void Dispose()
        {
            DetachDatabase();

            if (File.Exists(_fileName + ".mdf")) File.Delete(_fileName + ".mdf");
            if (File.Exists(_fileName + "_log.ldf")) File.Delete(_fileName + "_log.ldf");
        }

        public void RecreateTestData()
        {
            ExecuteQuery(_script);
        }

        public void ExecuteQuery(string script)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText = script;
                cmd.ExecuteNonQuery();
            }
        }

        private bool DetachDatabase()
        {
            try
            {
                using (var connection = new SqlConnection(LocalDbMaster))
                {
                    connection.Open();
                    var cmd = connection.CreateCommand();
                    // http://stackoverflow.com/questions/16243828/how-to-detach-a-localdb-sql-server-express-file-in-code
                    cmd.CommandText = String.Format("alter database {0} set offline with rollback immediate", _databaseName);
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = String.Format("exec sp_detach_db '{0}', 'true'", _databaseName);
                    cmd.ExecuteNonQuery();
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void CreateDatabase()
        {
            Dispose();

            using (var connection = new SqlConnection(LocalDbMaster))
            {
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText = string.Format("CREATE DATABASE {0} ON (NAME = N'{0}', FILENAME = '{1}.mdf')",
                    _databaseName,
                    _fileName);
                cmd.ExecuteNonQuery();
            }

            ExecuteQuery(_schema);
        }
    }
}


//using System;
//using System.IO;
//using System.Collections.Generic;
//using System.Linq;
//using System.Data.SqlClient;
//using System.Text;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using LinqEditor.Backend;
//using System.Windows.Forms;
//
//namespace LinqEditor.Core.Templates.Test.SqlServer
//{
//    [TestClass]
//    public class SchemaTests
//    {
//        static TestDatabase database;
//
//        #region Database SQL
//        static string DbSchema = @"
//            create table Foo (
//                Id int PRIMARY KEY,
//                Description Text
//            );";
//        static string DbScript = @"
//            delete Foo;
//            insert into Foo(Id,Description) values(0, 'Foo 0');
//            insert into Foo(Id,Description) values(1, 'Foo 1');
//            insert into Foo(Id,Description) values(2, 'Foo 2');
//            insert into Foo(Id,Description) values(3, 'Foo 3');";
//        #endregion
//
//
//        [ClassInitialize]
//        public static void Setup(TestContext context)
//        {
//            
//            database = new TestDatabase("UnitTest", DbSchema, DbScript);
//        }
//
//        [TestMethod]
//        public void Can_Execute_Query()
//        {
//            //var comp = new CSharpCodeService();
//            
//            //var generator = new LinqEditor.Generator.Query.SqlServer();
//            //generator.ConnectionString = database.ConnectionString;
//            //generator.SourceCode = "Foo.Dump();";
//            //var src = generator.TransformText();
//            //var assembly = comp.Run(src, generator.SourceCode);
//            //var programType = assembly.GetType(string.Format("{0}.Program", generator.GeneratedNamespace));
//            //var instance = Activator.CreateInstance(programType) as IQueryUnit;
//            //instance.Execute();
//            
//            //var dt = instance.Result.Get().FirstOrDefault();
//        }
//
//        [TestInitialize]
//        public void TestInitialize()
//        {
//            database.RecreateTestData();
//        }
//
//        [ClassCleanup]
//        public static void Cleanup()
//        {
//            database.Dispose();
//        }
//    }
//}