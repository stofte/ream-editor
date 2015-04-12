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
        // test store
        static IDictionary<string, string> _stringFiles = new Dictionary<string, string>();
        static IDictionary<string, byte[]> _binaryFiles = new Dictionary<string, byte[]>();

        static Func<string, bool> _defaultExistsProvider = (path) => File.Exists(path);
        static Func<string, byte[]> _defaultReadAllBytesProvider = (path) => File.ReadAllBytes(path);
        static Func<string, string> _defaultReadAllTextProvider = (path) => File.ReadAllText(path);
        static Action<string, string> _defaultWriteAllTextProvider = (path, contents) => File.WriteAllText(path, contents);

        static Func<string, bool> _stubExistsProvider = (path) =>
        {
            return _stringFiles.ContainsKey(path) || _binaryFiles.ContainsKey(path);
        };
        static Func<string, byte[]> _stubReadAllBytesProvider = (path) => _binaryFiles.ContainsKey(path) ? _binaryFiles[path] : null;
        static Func<string, string> _stubReadAllTextProvider = (path) => _stringFiles.ContainsKey(path) ? _stringFiles[path] : null;
        static Action<string, string> _stubWriteAllTextProvider = (path, contents) =>
        {
            _stringFiles[path] = contents;
        };

        static Func<string, bool> _existsProvider = _defaultExistsProvider;
        static Func<string, byte[]> _readAllBytesProvider = _defaultReadAllBytesProvider;
        static Func<string, string> _readAllTextProvider = _defaultReadAllTextProvider;
        static Action<string, string> _writeAllTextProvider = _defaultWriteAllTextProvider;

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

        public static void Mode(bool enableStub)
        {
            if (enableStub)
            {
                ExistsProvider = _stubExistsProvider;
                ReadAllBytesProvider = _stubReadAllBytesProvider;
                ReadAllTextProvider = _stubReadAllTextProvider;
                WriteAllTextProvider = _stubWriteAllTextProvider;
            }
            else
            {
                ExistsProvider = _defaultExistsProvider;
                ReadAllBytesProvider = _defaultReadAllBytesProvider;
                ReadAllTextProvider = _defaultReadAllTextProvider;
                WriteAllTextProvider = _defaultWriteAllTextProvider;
            }
        }
    }
}
