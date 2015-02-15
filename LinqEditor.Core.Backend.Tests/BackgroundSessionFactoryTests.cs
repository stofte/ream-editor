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
using LinqEditor.Core.CodeAnalysis.Services;
using Castle.Windsor.Installer;
using LinqEditor.Core.Settings;

namespace LinqEditor.Core.Backend.Tests
{
    [TestFixture]
    public class BackgroundSessionFactoryTests
    {
        [Test]
        public void Can_Create_BackgroundSessions()
        {
            var container = new WindsorContainer();
            container.AddFacility<TypedFactoryFacility>();

            container.Install(FromAssembly.Containing<IConnectionStore>()); // core
            container.Install(FromAssembly.Containing<ITemplateService>()); // core.templates
            container.Install(FromAssembly.Containing<IBackgroundSessionFactory>()); // core.backend
            container.Install(FromAssembly.Containing<ISqlSchemaProvider>()); // core.schema
            container.Install(FromAssembly.Containing<ITemplateCodeAnalysis>()); // core.codeanalysis
            
            var factory = container.Resolve<IBackgroundSessionFactory>();

            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var session1 = factory.Create(id1);
            var session2 = factory.Create(id1);
            var session3 = factory.Create(id2);

            Assert.IsNotNull(session1);
            Assert.AreEqual(session1.Id, id1);
            Assert.AreSame(session1, session2);
            Assert.AreNotSame(session1, session3);
            Assert.AreNotSame(session2, session3);
        }
    }
}
