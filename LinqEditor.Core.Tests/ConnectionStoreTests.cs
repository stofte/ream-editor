using LinqEditor.Core.Settings;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.Tests
{
    [TestFixture]
    public class ConnectionStoreTests
    {
        [TestCase(
            @"Data Source=.\sqlexpress;Integrated Security=True;Initial Catalog=Opera56100DB",
            @".\sqlexpress.Opera56100DB (Integrated Security)", 
            Description = "with catalog")]
        public void Connection_ToString_Parse_ConnectionString(string connStr, string toStringed)
        {
            var conn = new Connection { ConnectionString = connStr };

            var str = conn.ToString();

            Assert.AreEqual(toStringed, str);
        }
    }
}
