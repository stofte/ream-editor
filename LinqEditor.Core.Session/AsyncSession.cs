using LinqEditor.Core.CodeAnalysis.Services;
using LinqEditor.Core.Containers;
using LinqEditor.Core.Models.Analysis;
using LinqEditor.Core.Models.Editor;
using LinqEditor.Core.Schema.Services;
using LinqEditor.Core.Settings;
using LinqEditor.Core.Templates;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LinqEditor.Core.Session
{
    // ok to use Task.Run when "just" offloading from UI thread
    // http://blogs.msdn.com/b/pfxteam/archive/2012/04/12/10293335.aspx?Redirected=true
    public class AsyncSession : Session, IAsyncSession
    {
        public AsyncSession(Guid id, ISchemaProvider schemaProvider, ITemplateService generator, 
            IIsolatedCodeContainerFactory codeContainerFactory, IIsolatedDatabaseContainerFactory databaseContainerFactory, 
            IConnectionStore connectionStore, ITemplateCodeAnalysis codeAnalysis) :
            base(id, schemaProvider, generator, codeContainerFactory, databaseContainerFactory, connectionStore, codeAnalysis) { }

        public async Task<InitializeResult> InitializeAsync(Guid id)
        {
            return await Task.Run(() => Initialize(id));
        }

        public async Task<InitializeResult> ReinitializeAsync()
        {
            return await Task.Run(() => Reinitialize());
        }

        public async Task<ExecuteResult> ExecuteAsync(string sourceFragment)
        {
            return await Task.Run(() => Execute(sourceFragment));
        }

        public async Task<ExecuteResult> ExecuteAsync(string sourceFragment, CancellationToken ct)
        {
            return await Task.Run(() => Execute(sourceFragment, ct));
        }

        public async Task<LoadAppDomainResult> LoadAppDomainAsync()
        {
            return await Task.Run(() => LoadAppDomain());
        }

        public async Task<AnalysisResult> AnalyzeAsync(string sourceFragment, int updateIndex)
        {
            return await Task.Run(() => Analyze(sourceFragment, updateIndex));
        }

        public async Task<AnalysisResult> AnalyzeAsync(string sourceFragment)
        {
            return await Task.Run(() => Analyze(sourceFragment));
        }
    }
}
