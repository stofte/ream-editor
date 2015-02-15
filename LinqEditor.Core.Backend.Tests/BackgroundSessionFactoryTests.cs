using Castle.Facilities.TypedFactory;
using Castle.Windsor;
using Castle.Windsor.Installer;
using LinqEditor.Core.CodeAnalysis.Services;
using LinqEditor.Core.Containers;
using LinqEditor.Core.Helpers;
using LinqEditor.Core.Schema.Services;
using LinqEditor.Core.Settings;
using LinqEditor.Core.Templates;
using NUnit.Framework;
using System;
using System.Diagnostics;
using System.Threading;

namespace LinqEditor.Core.Backend.Tests
{
    [TestFixture]
    public class BackgroundSessionFactoryTests
    {
        WindsorContainer _container;

        [TestFixtureSetUp]
        public void Initialize()
        {
            _container = new WindsorContainer();
            _container.AddFacility<TypedFactoryFacility>();

            _container.Install(FromAssembly.Containing<IConnectionStore>()); // core
            _container.Install(FromAssembly.Containing<ITemplateService>()); // core.templates
            _container.Install(FromAssembly.Containing<IBackgroundSessionFactory>()); // core.backend
            _container.Install(FromAssembly.Containing<ISqlSchemaProvider>()); // core.schema
            _container.Install(FromAssembly.Containing<ITemplateCodeAnalysis>()); // core.codeanalysis
        }

        [Test]
        public void Can_Create_BackgroundSessions()
        {
            var factory = _container.Resolve<IBackgroundSessionFactory>();

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


        [Test]
        public async void Session_Returns_DurationMs_For_Main_Methods()
        {
            DebugLogger.Log("test start");
            var factory = _container.Resolve<IBackgroundSessionFactory>();
            var id = Guid.NewGuid();
            var src = "Write(\"foo\");";
            var session = factory.Create(id);
            var initRes = await session.InitializeAsync(ConnectionStore.CodeId);
            var loadRes = await session.LoadAppDomainAsync();
            var execRes = await session.ExecuteAsync(src);
            Assert.Greater(initRes.DurationMs, 0);
            Assert.Greater(loadRes.DurationMs, 0);
            Assert.Greater(execRes.DurationMs, 0);
        }

        [Test]
        public async void Can_Create_And_Cancel_Background_Code_Session()
        {
            DebugLogger.Log("test start");
            // todo: this test fails randomly (less often now that debug code is inserted ...)
            var factory = _container.Resolve<IBackgroundSessionFactory>();
            var id = Guid.NewGuid();
            var ms = 5 * 60 * 1000;
            var src1 = string.Format("System.Threading.Thread.Sleep({0});", ms);
            var src2 = "Write(\"foo\");";
            var session = factory.Create(id);
            await session.InitializeAsync(ConnectionStore.CodeId);

            var watch = new Stopwatch();
            
            await session.LoadAppDomainAsync();
            watch.Start();
            var cts = new CancellationTokenSource();
            cts.CancelAfter(1000);
            Assert.AreNotSame(cts.Token, CancellationToken.None);
            var result = await session.ExecuteAsync(src1, cts.Token);
            Assert.AreEqual("Cancelled", result.CodeOutput);
           
            // assert we didn't run the full duration
            Assert.GreaterOrEqual(ms, watch.ElapsedMilliseconds);
           
            // reinitialize session and try another execute
            await session.ReinitializeAsync();
            //Thread.Sleep(1000);
            var result2 = await session.ExecuteAsync(src2);
            Assert.AreEqual("foo", result2.CodeOutput);
        }
    }
}
