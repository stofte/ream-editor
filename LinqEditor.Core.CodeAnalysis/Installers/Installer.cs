using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using LinqEditor.Core.CodeAnalysis.Helpers;
using LinqEditor.Core.CodeAnalysis.Services;

namespace LinqEditor.Core.CodeAnalysis.Installers
{
    public class Installer : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<ITemplateCodeAnalysis>()
                                        .ImplementedBy<TemplateCodeAnalysis>()
                                        .LifestyleTransient());
            container.Register(Component.For<IDocumentationService>()
                                        .ImplementedBy<DocumentationService>()
                                        .LifestyleSingleton());
            container.Register(Component.For<ISymbolStore>()
                                        .ImplementedBy<SymbolStore>()
                                        .LifestyleSingleton());
            container.Register(Component.For<IToolTipHelper>()
                             .ImplementedBy<ToolTipHelper>()
                             .LifestyleTransient());
            container.Register(Component.For<IToolTipHelperFactory>().AsFactory());
        }
    }
}
