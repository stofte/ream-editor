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
    public class ApplicationSettingsTests
    {
        string path = PathUtility.ApplicationDirectory + ApplicationSettings.FileName;

        [TestFixtureSetUp]
        [TestFixtureTearDown]
        public void InitializeAndCleanup()
        {
            // delete any previous test file
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        [Test]
        public void Adding_Connection_Persists_Settings()
        {
            var app = new ConnectionStore();
            app.AddConnection(new Connection { ConnectionString = "foo" });

            Assert.IsTrue(File.Exists(path));
        }
    }
}
