using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using LinqEditor.Core.Session;
using LinqEditor.Core.Scopes;

namespace LinqEditor.Core.Session.Installers
{
    public class Installer : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<IAsyncSessionFactory>()
                                        .AsFactory());
            container.Register(Component.For<IAsyncSession>()
                                        .ImplementedBy<AsyncSession>()
                                        .LifestyleScoped<IdScopeAccessor>());
        }
    }
}
