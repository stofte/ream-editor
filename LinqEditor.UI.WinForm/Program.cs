using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using LinqEditor.Utility;

namespace LinqEditor.UI.WinForm
{
    static class Program
    {
        public static string CachePath { get; set; }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            CachePath = Utility.Utility.CachePath();

            if (!Directory.Exists(CachePath))
            {
                Directory.CreateDirectory(CachePath);
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new WinForm.Main());
        }
    }
}
