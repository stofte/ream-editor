using LinqEditor.Core.Models;
using LinqEditor.Test.Common;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.Tests
{
    [TestFixture]
    public class SQLiteFileConnectionTests
    {
        [TestCase(SQLiteTestData.Connstr1, true)]
        [TestCase("", true)]
        [TestCase(" ", true)]
        [TestCase(null, false)]
        public void Validation_Is_False_For_Null_Only(string path, bool result)
        {
            var conn = new SQLiteFileConnection { ConnectionString = path };

            Assert.AreEqual(result, conn.IsValidConnectionString);
        }

        [TestCase(SQLiteTestData.Connstr1, @"foo.sqlite (C:\)")]
        [TestCase(SQLiteTestData.Connstr2, @"data.db (\\some.where\some\folder)")]
        [TestCase(null, @"")]
        public void ToString_Renders_Expected_Format(string input, string expected)
        {
            var conn = new SQLiteFileConnection
            {
                ConnectionString = input
            };

            Assert.AreEqual(expected, conn.ToString());
        }
    }
}
