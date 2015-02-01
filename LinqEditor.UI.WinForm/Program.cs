using Castle.MicroKernel.Lifestyle;
using Castle.Windsor;
using Castle.Windsor.Installer;
using LinqEditor.Common;
using LinqEditor.UI.WinForm.Forms;
using System;
using System.IO;
using System.Windows.Forms;

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

            var path = PathUtility.CachePath;

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
