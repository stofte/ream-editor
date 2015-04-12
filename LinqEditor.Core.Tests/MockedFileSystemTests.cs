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

        [TestFixtureTearDown]
        public void Cleanup()
        {
            FileSystem.Mode(enableStub: false);
        }

        // these tests are duplicated from FileSystemTests, but runs different codepath
        [Test]
        public void ReadAllBytes_With_Non_Existant_Path_Throws()
        {
            Assert.Throws<FileNotFoundException>(() => FileSystem.ReadAllBytes("not.here.txt"));
        }

        [Test]
        public void WriteAllBytes_Contents_Is_Not_Mandatory()
        {
            FileSystem.WriteAllBytes("foo.bin", null);
        }

        [TestCase("foo.txt", "foo.txt", true)]
        [TestCase("foo.txt", "foo", false)]
        public void WriteAllText_And_Exists_Are_Consistent(string file1, string file2, bool outcome)
        {
            FileSystem.WriteAllText(file1, "some contents");

            Assert.AreEqual(outcome, FileSystem.Exists(file2));
        }

        [TestCase("foo.txt", "foo.txt", "foo baz", true)]
        public void WriteAllText_And_ReadAllText_Are_Consistent(string file1, string file2, string contents, bool outcome)
        {
            FileSystem.WriteAllText(file1, contents);

            var equalVal = FileSystem.ReadAllText(file1) == contents;

            Assert.AreEqual(outcome, equalVal);
        }

        [TestCase("foo.bin", "foo.bin", true)]
        [TestCase("foo.bin", "foo", false)]
        public void WriteAllBytes_And_Exists_Are_Consistent(string file1, string file2, bool outcome)
        {
            FileSystem.WriteAllBytes(file1, new byte[] { 0x2a });

            Assert.AreEqual(outcome, FileSystem.Exists(file2));
        }

        [TestCase("foo.bin", "foo.bin", 0x2a, true)]
        public void WriteAllBytes_And_ReadAllBytes_Are_Consistent(string file1, string file2, int contents, bool outcome)
        {
            var data = new byte[] { (byte)contents };
            FileSystem.WriteAllBytes(file1, data);

            var equalVal = FileSystem.ReadAllBytes(file2).SequenceEqual(data);

            Assert.AreEqual(outcome, equalVal);
        }
    }
}
