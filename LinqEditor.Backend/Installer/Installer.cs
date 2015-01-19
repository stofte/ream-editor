using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using LinqEditor.Backend.Compiler;
using LinqEditor.Backend.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Backend.Installer
{
    public class Installer : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<ICSharpCompiler>().ImplementedBy<CSharpCompiler>());
            container.Register(Component.For<ISession>().ImplementedBy<Session.Session>());
        }
    }
}
