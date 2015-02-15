using LinqEditor.Core.Models.Analysis;
using LinqEditor.Core.Models.Editor;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LinqEditor.Core.Backend
{
    public interface IBackgroundSession
    {
        Task<InitializeResult> InitializeAsync(Guid id);
        Task<InitializeResult> ReinitializeAsync();
        Task<ExecuteResult> ExecuteAsync(string sourceFragment);
        Task<ExecuteResult> ExecuteAsync(string sourceFragment, CancellationToken ct);
        Task<LoadAppDomainResult> LoadAppDomainAsync();
        Task<AnalysisResult> AnalyzeAsync(string sourceFragment, int updateIndex);
        Task<AnalysisResult> AnalyzeAsync(string sourceFragment);
        Guid Id { get; }
    }
}
