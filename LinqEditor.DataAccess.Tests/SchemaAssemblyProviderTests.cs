using LinqEditor.Core.Models.Database;
using LinqEditor.Core.Settings;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.DataAccess.Tests
{
    [TestFixture]
    public class SchemaAssemblyProviderTests
    {
        [Test]
        public async void Can_Load_DatabaseSchema_Instance()
        {
            var provider = new SchemaAssemblyProvider();
            var conn = new Connection();
            DatabaseSchema schema = await provider.Load(conn);

            Assert.IsNotNull(schema);
        }
    }
}
