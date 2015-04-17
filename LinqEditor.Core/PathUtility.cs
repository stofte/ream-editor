using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LinqEditor.Core
{
    public class PathUtility
    {
        static Func<string> _defaultCachePathProvider = () =>
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\LinqEditor\cache\";
        };

        static Func<string> _defaultTempPathProvider = () =>
        {
            return Path.GetTempPath();
        };

        static Func<string> _defaultApplicationDirectoryProvider = () =>
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        };

        static Func<string> _defaultCurrentPathProvider = () =>
        {
            return Environment.CurrentDirectory + @"\";
        };

        static Func<string> _cachePathProvider = _defaultCachePathProvider;
        static Func<string> _tempPathProvider = _defaultTempPathProvider;
        static Func<string> _applicationDirectoryProvider = _defaultApplicationDirectoryProvider;
        static Func<string> _currentPathProvider = _defaultCurrentPathProvider;

        public static string CachePath 
        { 
            get { return _cachePathProvider(); } 
        }

        public static string TempPath
        {
            get { return _tempPathProvider(); }
        }

        public static string ApplicationDirectory
        {
            get { return _applicationDirectoryProvider(); }
        }

        public static string CurrentPath
        {
            get { return _currentPathProvider(); }
        }

        public static Func<string> CachePathProvider
        {
            get { return _cachePathProvider; }
            set { _cachePathProvider = value; }
        }

        public static Func<string> TempPathProvider
        {
            get { return _tempPathProvider; }
            set { _tempPathProvider = value; }
        }

        public static Func<string> ApplicationDirectoryProvider
        {
            get { return _applicationDirectoryProvider; }
            set { _applicationDirectoryProvider = value; }
        }

        public static Func<string> CurrentPathProvider
        {
            get { return _currentPathProvider; }
            set { _currentPathProvider = value; }
        }

        public static void Reset()
        {
            _cachePathProvider = _defaultCachePathProvider;
            _tempPathProvider = _defaultTempPathProvider;
            _applicationDirectoryProvider = _defaultApplicationDirectoryProvider;
            _currentPathProvider = _defaultCurrentPathProvider;
        }
    }
}
