using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using LinqEditor.Core.CodeAnalysis.Editor;
using LinqEditor.Core.CodeAnalysis.Repositories;

namespace LinqEditor.Core.CodeAnalysis.Installers
{
    public class Installer : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<IBackgroundCompletion>()
                                        .ImplementedBy<BackgroundCompletion>()
                                        .LifestyleScoped());
            container.Register(Component.For<ISemanticStore>()
                            .ImplementedBy<SemanticStore>()
                            .LifestyleScoped());
            
        }
    }
}
