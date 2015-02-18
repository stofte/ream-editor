using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.UI.WinForm.Resources
{
    public class LibraryExtractor
    {
        public Assembly OutputNativeLibrary(string name)
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name))
            {

                Byte[] assemblyData = new Byte[stream.Length];

                stream.Read(assemblyData, 0, assemblyData.Length);

                return Assembly.Load(assemblyData);
            }
        }
    }
}
