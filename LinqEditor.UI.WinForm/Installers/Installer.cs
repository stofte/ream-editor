using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor.Installer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Installers
{
    public class Installer : IWindsorInstaller
    {
        public void Install(Castle.Windsor.IWindsorContainer container, IConfigurationStore store)
        {
            container.Install(FromAssembly.Named("LinqEditor.Core.CodeAnalysis"));
            container.Install(FromAssembly.Named("LinqEditor.Core.Templates"));
            container.Install(FromAssembly.Named("LinqEditor.Core.Backend"));
            container.Install(FromAssembly.Named("LinqEditor.Core.Schema"));
        }
    }
}
