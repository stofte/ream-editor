using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using LinqEditor.Core.Context;

namespace LinqEditor.Core.Installers
{
    public class Installer : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<Settings.ISchemaStore>()
                                        .ImplementedBy<Settings.SchemaStore>());
            container.Register(Component.For<IContext>()
                                        .ImplementedBy<Context.Context>()
                                        .LifestyleScoped());
        }
    }
}
