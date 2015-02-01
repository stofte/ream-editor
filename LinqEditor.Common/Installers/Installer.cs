using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using LinqEditor.Common.Context;

namespace LinqEditor.Common.Installers
{
    public class Installer : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<IContext>()
                                        .ImplementedBy<Context.Context>()
                                        .LifestyleScoped());
        }
    }
}
