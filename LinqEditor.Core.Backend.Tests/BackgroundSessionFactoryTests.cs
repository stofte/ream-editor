using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Lifestyle;
using Castle.Windsor;
using LinqEditor.Core.Containers;
using LinqEditor.Core.Context;
using LinqEditor.Core.Schema.Services;
using LinqEditor.Core.Scopes;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqEditor.Core.Templates;
using Moq;

namespace LinqEditor.Core.Backend.Tests
{
    [TestFixture]
    public class BackgroundSessionFactoryTests
    {
        [Test]
        public void Can_Create_BackgroundSession()
        {
            var container = new WindsorContainer();
            container.AddFacility<TypedFactoryFacility>();

            var mockedSchemaStore = new Mock<Settings.ISchemaStore>();
            
            container.Register(Component.For<Settings.ISchemaStore>()
                                        .Instance(mockedSchemaStore.Object));

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
            
            container.Register(Component.For<ISqlSchemaProvider>().ImplementedBy<SqlSchemaProvider>());
            container.Register(Component.For<ITemplateService>().ImplementedBy<TemplateService>());
            container.Register(Component.For<IBackgroundSessionFactory>()
                            .AsFactory());
            container.Register(Component.For<IBackgroundSession>()
                                        .ImplementedBy<BackgroundSession>()
                                        .LifestyleScoped());

            var factory = container.Resolve<IBackgroundSessionFactory>();

            // session is scope dependent
            using (var scope = container.BeginScope())
            {
                var session = factory.Create();
                factory.Release(session);
            }
        }
    }
}
