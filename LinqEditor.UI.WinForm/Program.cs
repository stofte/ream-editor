using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using LinqEditor.Common;
using Castle.Windsor;
using Castle.Windsor.Installer;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Lifestyle;
using LinqEditor.UI.WinForm.Forms;

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

            var path = Common.Utility.CachePath();

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            using (var container = new WindsorContainer().Install(FromAssembly.This()))
            {
                using (var scope = container.BeginScope())
                {
                    var mainForm = container.Resolve<Main>();
                    Application.Run(mainForm);
                    container.Release(mainForm);
                }
            }
        }
    }
}
