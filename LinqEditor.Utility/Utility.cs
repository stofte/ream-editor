using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LinqEditor.Utility
{
    public class Utility
    {
        public static string CachePath()
        {
            var doc = XDocument.Load(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
            var paths = doc.Descendants(XName.Get("probing", "urn:schemas-microsoft-com:asm.v1"));
            return AppDomain.CurrentDomain.BaseDirectory + paths.First().Attribute("privatePath").Value + "\\";
        }
    }
}
