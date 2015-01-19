using Castle.MicroKernel.Registration;
using Castle.Windsor.Installer;
using LinqEditor.Backend.Compiler;
using LinqEditor.Backend.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Installers
{
    public class Installer : IWindsorInstaller
    {
        public void Install(Castle.Windsor.IWindsorContainer container, Castle.MicroKernel.SubSystems.Configuration.IConfigurationStore store)
        {
            container.Install(FromAssembly.Named("LinqEditor.Backend"));
            container.Install(FromAssembly.Named("LinqEditor.Schema"));
            container.Install(FromAssembly.Named("LinqEditor.Generator"));
        }
    }
}
