using LinqEditor.Core.Models.Editor;
using LinqEditor.Core.Settings;
using LinqEditor.Core.Context;
using LinqEditor.Core.Schema.Services;
using LinqEditor.Core.Templates;
using System.Threading.Tasks;
using LinqEditor.Core.Containers;

namespace LinqEditor.Core.Backend
{
    // ok to use Task.Run when "just" offloading from UI thread
    // http://blogs.msdn.com/b/pfxteam/archive/2012/04/12/10293335.aspx?Redirected=true
    public class BackgroundSession : Session, IBackgroundSession
    {
        public BackgroundSession(ISqlSchemaProvider schemaProvider, ITemplateService generator, ISchemaStore userSettings, IContext context, 
            IIsolatedCodeContainerFactory codeContainerFactory, IIsolatedDatabaseContainerFactory databaseContainerFactory, IContainerMapper containerMapper) :
            base(schemaProvider, generator, userSettings, context, codeContainerFactory, databaseContainerFactory, containerMapper) { }

        public async Task<InitializeResult> InitializeAsync(string connectionString)
        {
            return await Task.Run(() => Initialize(connectionString));
        }

        public async Task<InitializeResult> InitializeAsync()
        {
            return await Task.Run(() => Initialize());
        }

        public async Task<ExecuteResult> ExecuteAsync(string sourceFragment)
        {
            return await Task.Run(() => Execute(sourceFragment));
        }

        public async Task<LoadAppDomainResult> LoadAppDomainAsync()
        {
            return await Task.Run(() => LoadAppDomain());
        }
    }
}
