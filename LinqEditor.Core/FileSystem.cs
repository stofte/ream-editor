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

        static byte[] _nullByteSentinel = Guid.NewGuid().ToByteArray();
        static string _nullStringSentinel = Guid.NewGuid().ToString();

        static Func<string, bool> _defaultExistsProvider = (path) => File.Exists(path);
        static Func<string, byte[]> _defaultReadAllBytesProvider = (path) => File.ReadAllBytes(path);
        static Func<string, string> _defaultReadAllTextProvider = (path) => File.ReadAllText(path);
        static Action<string, string> _defaultWriteAllTextProvider = (path, contents) => File.WriteAllText(path, contents);
        static Action<string, byte[]> _defaultWriteAllBytesProvider = (path, contents) => File.WriteAllBytes(path, contents);

        static Func<string, bool> _stubExistsProvider = (path) =>
        {
            return _stringFiles.ContainsKey(path) || _binaryFiles.ContainsKey(path);
        };
        static Func<string, byte[]> _stubReadAllBytesProvider = (path) =>
        {
            if (!_binaryFiles.ContainsKey(path))
            {
                throw new FileNotFoundException();
            }
            var val = _binaryFiles[path];
            return val == _nullByteSentinel ? null : val;
        };

        static Func<string, string> _stubReadAllTextProvider = (path) =>
        {
            if (_stringFiles.ContainsKey(path))
            {
                var val = _stringFiles[path];
                return val == _nullStringSentinel ? null : val;
            }
            return null;
        };
        static Action<string, string> _stubWriteAllTextProvider = (path, contents) =>
        {
            var val = contents ?? _nullStringSentinel;
            _stringFiles[path] = val;
        };

        static Action<string, byte[]> _stubWriteAllBytesProvider = (path, contents) =>
        {
            var val = contents ?? _nullByteSentinel;
            _binaryFiles[path] = val;
        };

        static Func<string, bool> _existsProvider = _defaultExistsProvider;
        static Func<string, byte[]> _readAllBytesProvider = _defaultReadAllBytesProvider;
        static Func<string, string> _readAllTextProvider = _defaultReadAllTextProvider;
        static Action<string, string> _writeAllTextProvider = _defaultWriteAllTextProvider;
        static Action<string, byte[]> _writeAllBytesProvider = _defaultWriteAllBytesProvider;

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

        public static void WriteAllBytes(string filePath, byte[] contents)
        {
            _writeAllBytesProvider(filePath, contents);
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

        public static Action<string, byte[]> WriteAllBytesProvider
        {
            get { return _writeAllBytesProvider; }
            set { _writeAllBytesProvider = value; }
        }

        /// <summary>
        /// Toggles file system mode, and clear stub mode dictionaries
        /// </summary>
        /// <param name="enableStub">enables stub mode</param>
        public static void Mode(bool enableStub)
        {
            if (enableStub)
            {
                ExistsProvider = _stubExistsProvider;
                ReadAllBytesProvider = _stubReadAllBytesProvider;
                ReadAllTextProvider = _stubReadAllTextProvider;
                WriteAllTextProvider = _stubWriteAllTextProvider;
                WriteAllBytesProvider = _stubWriteAllBytesProvider;
            }
            else
            {
                ExistsProvider = _defaultExistsProvider;
                ReadAllBytesProvider = _defaultReadAllBytesProvider;
                ReadAllTextProvider = _defaultReadAllTextProvider;
                WriteAllTextProvider = _defaultWriteAllTextProvider;
                WriteAllBytesProvider = _defaultWriteAllBytesProvider;
            }
            Clear();
        }

        /// <summary>
        /// Clears internal dictionaries
        /// </summary>
        public static void Clear()
        {
            _binaryFiles.Clear();
            _stringFiles.Clear();
        }
    }
}
