using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using LinqEditor.Core.Containers;
using LinqEditor.Core.Scopes;
using NUnit.Framework;
using System;

namespace LinqEditor.Core.Tests
{
    [TestFixture]
    public class WindsorContainerTests
    {
        [Test]
        public void Can_Load_CodeContainer_From_Container()
        {
            var container = new WindsorContainer();
            container.AddFacility<TypedFactoryFacility>();
            container.Register(Component.For<IIsolatedCodeContainerFactory>().AsFactory());

            container.Register(Component.For<IIsolatedCodeContainer>()
                .ImplementedBy<IsolatedCodeContainer>()
                .LifestyleScoped<ContainerScopeAccessor>());

            var codeContainerFactory = container.Resolve<IIsolatedCodeContainerFactory>();
            var id = Guid.NewGuid();
            var instance1 = codeContainerFactory.Create(id);
            var instance2 = codeContainerFactory.Create(id);
            Assert.AreSame(instance1, instance2);
        }

        [Test]
        public void Can_Load_Different_DatabaseContainer_Instances()
        {
            var container = new WindsorContainer();
            container.AddFacility<TypedFactoryFacility>();

            container.Register(Component.For<IIsolatedDatabaseContainerFactory>().AsFactory());
            container.Register(Component.For<IIsolatedDatabaseContainer>()
                .ImplementedBy<IsolatedDatabaseContainer>()
                .LifestyleScoped<ContainerScopeAccessor>());

            var containerFactory = container.Resolve<IIsolatedDatabaseContainerFactory>();
            var dbId1 = Guid.NewGuid();
            var dbId2 = Guid.NewGuid();
            var instance1 = containerFactory.Create(dbId1);
            var instance2 = containerFactory.Create(dbId2);
            Assert.AreNotSame(instance1, instance2);
        }
    }
}
