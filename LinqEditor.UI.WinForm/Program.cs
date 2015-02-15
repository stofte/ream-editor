using Castle.Windsor;
using Castle.Windsor.Installer;
using LinqEditor.Core;
using LinqEditor.UI.WinForm.Forms;
using System;
using System.IO;
using System.Windows.Forms;

namespace LinqEditor.UI.WinForm
{
    class Program
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

            _container = new WindsorContainer().Install(FromAssembly.This());
            var mainForm = _container.Resolve<Main>();
            Application.Run(mainForm);
            _container.Release(mainForm);
        }

        static IWindsorContainer _container;

        public static IWindsorContainer Container
        {
            get { return _container; }
        }
    }
}
