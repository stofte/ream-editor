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
        const string Connstr1 = @"Data Source=.\sqlexpress;Integrated Security=True;Initial Catalog=Opera56100DB";
        const string Connstr2 = @"Data Source=sql.somewhere,1437; UID=yyy; PWD=xxx; DATABASE=mydbname";

        [TestCase(
            "",
            Connstr1,
            @".\sqlexpress.Opera56100DB (Integrated Security)", 
            Description = "with catalog")]
        [TestCase(
            "MyName",
            Connstr1,
            @"MyName .\sqlexpress.Opera56100DB (Integrated Security)",
            Description = "with catalog + display")]
        public void Connection_ToString_Parses_ConnectionString(string displayName, string connStr, string toStringed)
        {
            var conn = new Connection { ConnectionString = connStr, DisplayName = displayName, Kind = Models.Editor.ProgramType.Database };

            var str = conn.ToString();

            Assert.AreEqual(toStringed, str);
        }

        [TestCase(Connstr1, true)]
        public void Connection_Detects_Integrated_Security(string connStr, bool usingIntegratedSecurity)
        {
            var conn = new Connection { ConnectionString = connStr, Kind = Models.Editor.ProgramType.Database };

            Assert.AreEqual(conn.UsingIntegratedSecurity, usingIntegratedSecurity);
        }

        [TestCase(Connstr1, "Integrated Security")]
        [TestCase(Connstr2, "yyy")]
        [TestCase("bogus connection string", "")]
        public void Connection_Parses_Security_Context(string connStr, string securityString)
        {
            var conn = new Connection { ConnectionString = connStr, Kind = Models.Editor.ProgramType.Database };

            Assert.AreEqual(conn.DatabaseSecurity, securityString);
        }

        [TestCase(Connstr1, @".\sqlexpress")]
        [TestCase(Connstr2, @"sql.somewhere,1437")]
        [TestCase("bogus connection string", "")]
        public void Connection_Parses_DatabaseServer(string connStr, string serverString)
        {
            var conn = new Connection { ConnectionString = connStr, Kind = Models.Editor.ProgramType.Database };

            Assert.AreEqual(conn.DatabaseServer, serverString);
        }

        [TestCase(Connstr1, @"Opera56100DB")]
        [TestCase(Connstr2, @"mydbname")]
        [TestCase("bogus connection string", "")]
        public void Connection_Parses_InitialCatalog(string connStr, string catalog)
        {
            var conn = new Connection { ConnectionString = connStr, Kind = Models.Editor.ProgramType.Database };

            Assert.AreEqual(conn.InitialCatalog, catalog);
        }
    }
}
