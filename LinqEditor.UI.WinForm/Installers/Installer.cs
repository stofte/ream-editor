using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor.Installer;
using LinqEditor.UI.WinForm;
using LinqEditor.UI.WinForm.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Installers
{
    public class Installer : IWindsorInstaller
    {
        public void Install(Castle.Windsor.IWindsorContainer container, IConfigurationStore store)
        {
            container.AddFacility<TypedFactoryFacility>();

            container.Install(FromAssembly.Named("LinqEditor.Common"));
            container.Install(FromAssembly.Named("LinqEditor.Core.Backend"));
            container.Install(FromAssembly.Named("LinqEditor.Core.CodeAnalysis"));
            container.Install(FromAssembly.Named("LinqEditor.Core.Schema"));
            container.Install(FromAssembly.Named("LinqEditor.Core.Templates"));
            
            // install forms
            container.Register(Component.For<MainForm>());
            container.Register(Component.For<CodeEditor>());
            container.Register(Component.For<OutputPane>());
        }
    }
}
