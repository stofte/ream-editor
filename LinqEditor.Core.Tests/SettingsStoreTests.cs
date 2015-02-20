using LinqEditor.Core.Settings;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.Tests
{
    [TestFixture]
    public class SettingsStoreTests
    {
        string settingsPath = SettingsStore.FileName(typeof(SettingsStore));

        [TestFixtureSetUp, TestFixtureTearDown, SetUp]
        public void InitializeAndCleanup()
        {
            if (File.Exists(settingsPath))
            {
                File.Delete(settingsPath);
            }
        }

        [Test]
        public void Persists_Changes_ToDisc()
        {
            var store1 = SettingsStore.Instance;
            Assert.IsFalse(store1.MainMaximized);
            store1.MainMaximized = true;
            // reads the file in
            var store2 = SettingsStore.Instance;

            Assert.IsTrue(store2.MainMaximized);
        }

        [Test]
        public void Setting_Same_Value_Does_Not_Persist_Changes()
        {
            var store = SettingsStore.Instance;

            store.MainMaximized = true;
            var stamp1 = File.GetLastWriteTime(settingsPath);
            store.MainMaximized = true;
            var stamp2 = File.GetLastWriteTime(settingsPath);

            Assert.AreEqual(stamp1, stamp2);
        }
    }
}
