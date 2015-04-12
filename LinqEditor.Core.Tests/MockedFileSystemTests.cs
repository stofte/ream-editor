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
    public class MockedFileSystemTests
    {
        [TestFixtureSetUp]
        public void Initialize()
        {
            FileSystem.Mode(enableStub: true);
        }

        [TestCase("foo.txt", "foo.txt", true)]
        [TestCase("foo.txt", "foo", false)]
        public void Write_And_Exists_Are_Consistent(string file1, string file2, bool outcome)
        {
            FileSystem.WriteAllText(file1, "some contents");

            Assert.AreEqual(outcome, FileSystem.Exists(file2));
        }
    }
}
