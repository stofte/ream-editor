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
    public class FileSystemTests
    {
        static string _file1 = Path.GetFullPath("foo.txt");
        static string _file2 = Path.GetFullPath("bar.bin");

        [TestFixtureTearDown]
        public void Cleanup()
        {
            if (File.Exists(_file1)) File.Delete(_file1);
            if (File.Exists(_file2)) File.Delete(_file2);
        }

        [Test]
        public void Can_Write_And_Read_Text_File()
        {
            var str = "foo@bar";
            FileSystem.WriteAllText(_file1, str);
            var ret = FileSystem.ReadAllText(_file1);

            Assert.AreEqual(str, ret);
        }

        [Test]
        public void Can_Write_And_Check_Exists()
        {
            var str = "foo@bar";
            FileSystem.WriteAllText(_file1, str);
            Assert.IsTrue(FileSystem.Exists(_file1));
        }

        [Test]
        public void Can_Read_Binary_File()
        {
            var data = new byte[] { 0x2a };
            File.WriteAllBytes(_file2, data);

            var read = FileSystem.ReadAllBytes(_file2);

            Assert.AreEqual(data, read);
        }

        [Test]
        public void WriteAllText_File_Path_Is_Mandatory()
        {
            Assert.Throws<ArgumentNullException>(() => FileSystem.WriteAllText(null, "foo@bar"));
        }

        [Test]
        public void ReadAllBytes_File_Path_Is_Mandatory()
        {
            Assert.Throws<ArgumentNullException>(() => FileSystem.ReadAllBytes(null));
        }

        [Test]
        public void ReadAllText_File_Path_Is_Mandatory()
        {
            Assert.Throws<ArgumentNullException>(() => FileSystem.ReadAllText(null));
        }

        [Test]
        public void Exists_File_Path_Is_Not_Mandatory()
        {
            FileSystem.Exists(null);
        }
    }
}
