using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LinqEditor.Core
{
    public class PathUtility
    {
        public static string CachePath
        {
            get 
            {
                // should match AppDomain.CurrentDomain.BaseDirectory
                var doc = XDocument.Load(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
                var paths = doc.Descendants(XName.Get("probing", "urn:schemas-microsoft-com:asm.v1"));
                return AppDomain.CurrentDomain.BaseDirectory + paths.First().Attribute("privatePath").Value + "\\";
            }
        }

        public static string TempPath
        {
            get { return Path.GetTempPath(); }
        }

        public static string ApplicationDirectory
        {
            get { return AppDomain.CurrentDomain.BaseDirectory; }
        }

        public static string CurrentPath
        {
            get { return Environment.CurrentDirectory + @"\"; }
        }
    }
}
