using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LinqEditor
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "gendlls"))
            {
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "gendlls");
            }
            

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(LinqEditor.Main.Create());
        }
    }
}
