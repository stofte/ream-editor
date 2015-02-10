using Castle.MicroKernel.Registration;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using LinqEditor.Core.Containers;
using LinqEditor.Core.Context;
using LinqEditor.Core.Scopes;

namespace LinqEditor.Core.Installers
{
    public class Installer : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<Settings.ISchemaStore>()
                                        .ImplementedBy<Settings.SchemaStore>());
            container.Register(Component.For<Settings.IConnectionStore>()
                                        .ImplementedBy<Settings.ConnectionStore>()
                                        .UsingFactoryMethod(() => Settings.ConnectionStore.Instance)
                                        .LifestyleSingleton());

            container.Register(Component.For<IContext>()
                                        .ImplementedBy<Context.Context>()
                                        .LifestyleScoped());

            container.Register(Component.For<IContainerMapper>()
                                        .ImplementedBy<ContainerMapper>()
                                        .LifestyleSingleton());

            container.Register(Component.For<IIsolatedCodeContainerFactory>()
                                        .AsFactory());
            container.Register(Component.For<IIsolatedDatabaseContainerFactory>()
                                        .AsFactory());
            container.Register(Component.For<IIsolatedDatabaseContainer>()
                                        .ImplementedBy<IsolatedDatabaseContainer>()
                                        .LifestyleScoped<ContainerScopeAccessor>());
            container.Register(Component.For<IIsolatedCodeContainer>()
                                        .ImplementedBy<IsolatedCodeContainer>()
                                        .LifestyleScoped<ContainerScopeAccessor>());
        }
    }
}
