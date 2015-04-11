using LinqEditor.Test.Common.SQLite;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.Tests
{
    [TestFixture]
    public class SQLiteTests
    {
        [Test]
        public void Can_Create_Database_With_Test_Data()
        {
            long rowcount;

            using (var db = new SQLiteTestDb("test.sqlite"))
            {
                db.RecreateTestData();

                // verify using sqlite's api that the db contains something
                using (var conn = new SQLiteConnection(db.ConnectionString))
                {
                    conn.Open();
                    var cmd = new SQLiteCommand("select count(*) from Foo", conn);
                    rowcount = (long)cmd.ExecuteScalar();
                }
            }

            Assert.AreEqual(4, rowcount);
        }
    }
}
