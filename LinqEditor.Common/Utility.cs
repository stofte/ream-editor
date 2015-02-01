using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LinqEditor.Common
{
    public class Utility
    {
        public static string CachePath()
        {
            // should match AppDomain.CurrentDomain.BaseDirectory
            var doc = XDocument.Load(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
            var paths = doc.Descendants(XName.Get("probing", "urn:schemas-microsoft-com:asm.v1"));
            return AppDomain.CurrentDomain.BaseDirectory + paths.First().Attribute("privatePath").Value + "\\";
        }

        public static string TempPath()
        {
            return @"C:\Users\z8zse\Desktop\temp\";
        }
    }
}
