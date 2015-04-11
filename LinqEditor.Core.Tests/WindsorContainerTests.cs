using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.Installer;
using LinqEditor.Core.Containers;
using LinqEditor.Core.Models;
using LinqEditor.Core.Scopes;
using LinqEditor.Core.Settings;
using LinqEditor.Test.Common;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;

namespace LinqEditor.Core.Tests
{
    [TestFixture]
    public class WindsorContainerTests
    {
        [TestFixtureTearDown]
        public void Cleanup()
        {
            var p1 = ApplicationSettings.FileName(typeof(ConnectionStore));
            var p2 = ApplicationSettings.FileName(typeof(SettingsStore));
            if (File.Exists(p1)) File.Delete(p1);
            if (File.Exists(p2)) File.Delete(p2);
        }
        
        [Test]
        public void Can_Load_Different_DatabaseContainer_Instances()
        {
            var container = new WindsorContainer();
            container.AddFacility<TypedFactoryFacility>();
            container.Install(FromAssembly.Containing<IConnectionStore>()); // core

            var containerFactory = container.Resolve<IIsolatedDatabaseContainerFactory>();
            var instance1 = containerFactory.Create();
            var instance2 = containerFactory.Create();
            Assert.AreNotSame(instance1, instance2);
        }

        [Test]
        public void Can_Load_ConnectionStore_Using_Factory_Method()
        {
            // this generates the file on disc
            var id = Guid.NewGuid();
            var app = ConnectionStore.Instance;
            app.Add(new SqlServerConnection { Id = id, ConnectionString = DatabaseTestData.Connstr1, CachedSchemaFileName = "bar" });

            // hook up castle  registration
            var container = new WindsorContainer();
            container.Register(Component.For<ConnectionStore>().UsingFactoryMethod(() => ConnectionStore.Instance));

            var store = container.Resolve<ConnectionStore>();

            Assert.IsInstanceOf<ConnectionStore>(store);
            Assert.IsNotNull(store.Connections);
            Assert.GreaterOrEqual(store.Connections.Count(), 1); // must be at least one entry
        }
    }
}
