using Castle.Windsor.Installer;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using LinqEditor.Core.Backend.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Facilities.TypedFactory;

namespace LinqEditor.Core.Backend.Installers
{
    public class Installer : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<IBackgroundSessionFactory>()
                                        .AsFactory());
            container.Register(Component.For<IBackgroundSession>()
                                        .ImplementedBy<Repository.BackgroundSession>()
                                        .LifestyleScoped());
            container.Register(Component.For<Settings.ISchemaStore>()
                                        .ImplementedBy<Settings.SchemaStore>());
        }
    }
}
