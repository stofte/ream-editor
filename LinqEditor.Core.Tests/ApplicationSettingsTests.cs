using LinqEditor.Core.Settings;
using LinqEditor.Test.Common;
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
        string connStorePath = ConnectionStore.FileName(typeof(ConnectionStore));
        string settingsPath = SettingsStore.FileName(typeof(SettingsStore));

        [TestFixtureSetUp, TestFixtureTearDown, SetUp]
        public void InitializeAndCleanup()
        {
            // delete any previous test file, before and after all and any tests
            if (File.Exists(connStorePath))
            {
                File.Delete(connStorePath);
            }
            if (File.Exists(settingsPath))
            {
                File.Delete(settingsPath);
            }
        }

        [Test]
        public void Persisting_Connections_Will_Create_File()
        {
            var app = ConnectionStore.Instance;
            app.Add(new Connection { Id = Guid.NewGuid(), ConnectionString = DatabaseTestData.Connstr1, CachedSchemaFileName = "bar" });
            Assert.IsTrue(File.Exists(connStorePath));
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void Adding_Connection_With_No_Guid_Throws() 
        {
            var app = ConnectionStore.Instance;
            app.Add(new Connection { ConnectionString = DatabaseTestData.Connstr1 });
        }

        [Test]
        public void Connections_Is_Never_Null()
        {
            var app = ConnectionStore.Instance;
            Assert.IsNotNull(app.Connections);
        }
        
        [Test]
        public void Generated_Settings_File_Contains_Mapped_Values()
        {
            var app = ConnectionStore.Instance;
            var deleteGuid = Guid.NewGuid();
            app.Add(new Connection { Id = Guid.NewGuid(), ConnectionString = DatabaseTestData.Connstr1, CachedSchemaFileName = "bar" });
            app.Add(new Connection { Id = deleteGuid, ConnectionString = DatabaseTestData.Connstr2, CachedSchemaFileName = "baz" });
            app.Delete(app.Connections.Where(x => x.Id == deleteGuid).Single()); // delete the instance with deleteGuid 

            var fileData = File.ReadAllText(connStorePath);
            var instance = JsonConvert.DeserializeObject<ConnectionStore>(fileData);
            // reflect value out
            var reflectedValue = instance.GetType().GetField("_connections", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(instance) as IList<Connection>;
            Assert.AreEqual(1, reflectedValue.Count);
            Assert.AreEqual(DatabaseTestData.Connstr1, reflectedValue[0].ConnectionString);
            Assert.AreEqual("bar", reflectedValue[0].CachedSchemaFileName);
        }

        [Test]
        public void Detects_Error_Loading_File()
        {
            using (var file = File.CreateText(connStorePath))
            {
                file.Write(0x2a);
            }

            var app = ConnectionStore.Instance;

            Assert.IsTrue(app.LoadingError);
        }
    }
}
