using Castle.MicroKernel.Registration;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using LinqEditor.Core.Containers;
using LinqEditor.Core.Scopes;

namespace LinqEditor.Core.Installers
{
    public class Installer : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<Settings.ISchemaStore>()
                                        .ImplementedBy<Settings.SchemaStore>());

            // these classes touch files, so single instance only
            container.Register(Component.For<Settings.IConnectionStore>()
                                        .ImplementedBy<Settings.ConnectionStore>()
                                        .UsingFactoryMethod(() => Settings.ConnectionStore.Instance)
                                        .LifestyleSingleton());

            container.Register(Component.For<Settings.ISettingsStore>()
                                        .ImplementedBy<Settings.ISettingsStore>()
                                        .UsingFactoryMethod(() => Settings.SettingsStore.Instance)
                                        .LifestyleSingleton());
            
            //container.Register(Component.For<IContainerMapper>()
            //                            .ImplementedBy<ContainerMapper>());

            container.Register(Component.For<IIsolatedCodeContainerFactory>()
                                        .AsFactory());
            container.Register(Component.For<IIsolatedDatabaseContainerFactory>()
                                        .AsFactory());
            container.Register(Component.For<IIsolatedDatabaseContainer>()
                                        .ImplementedBy<IsolatedDatabaseContainer>()
                                        .LifestyleTransient());
                                        //.LifestyleScoped<IdScopeAccessor>());
            container.Register(Component.For<IIsolatedCodeContainer>()
                                        .ImplementedBy<IsolatedCodeContainer>()
                                        .LifestyleTransient());
                                        //.LifestyleScoped<IdScopeAccessor>());
        }
    }
}
