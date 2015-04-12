using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.Tests
{
    [TestFixture]
    public class PathUtilityTests
    {
        [TestFixtureTearDown]
        public void Cleanup()
        {
            PathUtility.Reset();
        }

        [Test]
        public void CachePath_Is_Not_Null()
        {
            Assert.IsNotNull(PathUtility.CachePath);
        }

        [Test]
        public void TempPath_Is_Not_Null()
        {
            Assert.IsNotNull(PathUtility.TempPath);
        }

        [Test]
        public void ApplicationDirectory_Is_Not_Null()
        {
            Assert.IsNotNull(PathUtility.ApplicationDirectory);
        }

        [Test]
        public void CurrentPath_Is_Not_Null()
        {
            Assert.IsNotNull(PathUtility.CurrentPath);
        }

        [Test]
        public void Can_Stub_CachePath()
        {
            var newPath = @"C:\cache\path";
            PathUtility.CachePathProvider = () => newPath;
            Assert.AreEqual(newPath, PathUtility.CachePath);
        }

        [Test]
        public void Can_stub_TempPath()
        {
            var newPath = @"C:\temp\path";
            PathUtility.TempPathProvider = () => newPath;
            Assert.AreEqual(newPath, PathUtility.TempPath);
        }

        [Test]
        public void Can_stub_ApplicationDirectory()
        {
            var newPath = @"C:\app\path";
            PathUtility.ApplicationDirectoryProvider = () => newPath;
            Assert.AreEqual(newPath, PathUtility.ApplicationDirectory);
        }

        [Test]
        public void Can_stub_CurrentPath()
        {
            var newPath = @"C:\app\path";
            PathUtility.CurrentPathProvider = () => newPath;
            Assert.AreEqual(newPath, PathUtility.CurrentPath);
        }
    }
}
