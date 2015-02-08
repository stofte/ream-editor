using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using LinqEditor.Core.Backend;

namespace LinqEditor.Core.Backend.Installers
{
    public class Installer : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<IBackgroundSession>()
                                        .ImplementedBy<BackgroundSession>()
                                        .LifestyleScoped());
        }
    }
}
