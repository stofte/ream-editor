using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using LinqEditor.Core.CodeAnalysis.Editor;

namespace LinqEditor.Core.CodeAnalysis.Installers
{
    public class Installer : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<IBackgroundCompletionFactory>()
                                        .AsFactory());
            container.Register(Component.For<IBackgroundCompletion>()
                                        .ImplementedBy<BackgroundCompletion>()
                                        .LifestyleScoped());
            
        }
    }
}
