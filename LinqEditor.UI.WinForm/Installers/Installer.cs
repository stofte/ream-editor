using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor.Installer;
using LinqEditor.UI.WinForm.Controls;
using LinqEditor.UI.WinForm.Forms;

namespace LinqEditor.Installers
{
    public class Installer : IWindsorInstaller
    {
        public void Install(Castle.Windsor.IWindsorContainer container, IConfigurationStore store)
        {
            container.AddFacility<TypedFactoryFacility>();

            container.Install(FromAssembly.Named("LinqEditor.Core"));
            container.Install(FromAssembly.Named("LinqEditor.Core.Backend"));
            container.Install(FromAssembly.Named("LinqEditor.Core.CodeAnalysis"));
            container.Install(FromAssembly.Named("LinqEditor.Core.Schema"));
            container.Install(FromAssembly.Named("LinqEditor.Core.Templates"));
            
            // install forms
            container.Register(Component.For<Main>()
                                        .LifestyleTransient());
            container.Register(Component.For<MainPanel>()
                                        .LifestyleTransient());
            container.Register(Component.For<ConnectionManager>()
                                        .LifestyleTransient());
            container.Register(Component.For<CodeEditor>()
                                        .LifestyleTransient());
            container.Register(Component.For<OutputPane>()
                                        .LifestyleTransient());
            container.Register(Component.For<ToolTip2>()
                                        .LifestyleSingleton());
        }
    }
}
