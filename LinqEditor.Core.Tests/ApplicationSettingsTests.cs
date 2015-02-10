using LinqEditor.Core.Settings;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.Tests
{
    [TestFixture]
    public class ApplicationSettingsTests
    {
        string path = PathUtility.ApplicationDirectory + ApplicationSettings.FileName;

        [TestFixtureSetUp]
        [TestFixtureTearDown]
        [SetUp]
        public void InitializeAndCleanup()
        {
            // delete any previous test file, before and after all and any tests
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        [Test]
        public void Persisting_Connections_Will_Create_File()
        {
            var app = ConnectionStore.Instance;
            app.Add(new Connection { Id = Guid.NewGuid(), ConnectionString = "foo", CachedSchemaFileName = "bar" });
            Assert.IsTrue(File.Exists(path));
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void Adding_Connection_With_No_Guid_Throws() 
        {
            var app = ConnectionStore.Instance;
            app.Add(new Connection { ConnectionString = "no id set" });
        }

        [Test]
        public void Generated_Settings_File_Contains_Mapped_Values()
        {
            var app = ConnectionStore.Instance;
            var deleteGuid = Guid.NewGuid();
            app.Add(new Connection { Id = Guid.NewGuid(), ConnectionString = "foo", CachedSchemaFileName = "bar" });
            app.Add(new Connection { Id = deleteGuid, ConnectionString = "qux", CachedSchemaFileName = "baz" });
            app.Delete(app.Connections.Where(x => x.Id == deleteGuid).Single()); // delete the instance with deleteGuid 

            var fileData = File.ReadAllText(path);
            var instance = JsonConvert.DeserializeObject<ConnectionStore>(fileData);
            // reflect value out
            var reflectedValue = instance.GetType().GetField("_connections", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(instance) as IList<Connection>;
            Assert.AreEqual(1, reflectedValue.Count);
            Assert.AreEqual("foo", reflectedValue[0].ConnectionString);
            Assert.AreEqual("bar", reflectedValue[0].CachedSchemaFileName);
        }
    }
}
