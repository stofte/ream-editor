using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core
{
    /// <summary>
    /// Abstracts the file system to allow tests to run without touching the filesystem.
    /// </summary>
    public static class FileSystem
    {
        static Func<string, bool> _existsProvider = (path) => File.Exists(path);
        static Func<string, byte[]> _readAllBytesProvider = (path) => File.ReadAllBytes(path);
        static Func<string, string> _readAllTextProvider = (path) => File.ReadAllText(path);
        static Action<string, string> _writeAllTextProvider = (path, contents) => File.WriteAllText(path, contents);

        public static bool Exists(string filePath)
        {
            return _existsProvider(filePath);
        }

        public static byte[] ReadAllBytes(string filePath)
        {
            return _readAllBytesProvider(filePath);
        }

        public static string ReadAllText(string filePath)
        {
            return _readAllTextProvider(filePath);
        }

        public static void WriteAllText(string filePath, string contents)
        {
            _writeAllTextProvider(filePath, contents);
        }

        public static Func<string, bool> ExistsProvider
        {
            get { return _existsProvider; }
            set { _existsProvider = value; }
        }

        public static Func<string, byte[]> ReadAllBytesProvider
        {
            get { return _readAllBytesProvider; }
            set { _readAllBytesProvider = value; }
        }

        public static Func<string, string> ReadAllTextProvider
        {
            get { return _readAllTextProvider; }
            set { _readAllTextProvider = value; }
        }

        public static Action<string, string> WriteAllTextProvider
        {
            get { return _writeAllTextProvider; }
            set { _writeAllTextProvider = value; }
        }
    }
}
