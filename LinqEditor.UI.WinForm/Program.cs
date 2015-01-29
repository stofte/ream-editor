using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using LinqEditor.Utility;
using Castle.Windsor;
using Castle.Windsor.Installer;
using Castle.Facilities.TypedFactory;

namespace LinqEditor.UI.WinForm
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var path = Utility.Utility.CachePath();

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            using (var container = new WindsorContainer().Install(FromAssembly.This()))
            {
                var mainForm = container.Resolve<MainForm>();
                Application.Run(mainForm);
                container.Release(mainForm);
            }
        }
    }
}
