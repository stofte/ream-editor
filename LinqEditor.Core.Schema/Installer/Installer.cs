using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using LinqEditor.Core.Schema.Services;

namespace LinqEditor.Core.Schema.Installer
{
    public class Installer : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<ISchemaProvider>().ImplementedBy<MSSQLServerSchemaProvider>());
        }
    }
}
