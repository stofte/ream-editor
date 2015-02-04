using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using LinqEditor.Core.Backend.Repository;

namespace LinqEditor.Core.Backend.Installers
{
    public class Installer : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<IBackgroundSession>()
                                        .ImplementedBy<Repository.BackgroundSession>()
                                        .LifestyleScoped());
            container.Register(Component.For<Settings.ISchemaStore>()
                                        .ImplementedBy<Settings.SchemaStore>());
        }
    }
}
