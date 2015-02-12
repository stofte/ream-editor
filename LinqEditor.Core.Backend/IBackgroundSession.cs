using LinqEditor.Core.Models.Analysis;
using LinqEditor.Core.Models.Editor;
using System;
using System.Threading.Tasks;

namespace LinqEditor.Core.Backend
{
    public interface IBackgroundSession
    {
        //Task<InitializeResult> InitializeAsync(string connectionString);
        //Task<InitializeResult> InitializeAsync();
        Task<InitializeResult> InitializeAsync(Guid id);
        Task<ExecuteResult> ExecuteAsync(string sourceFragment);
        Task<LoadAppDomainResult> LoadAppDomainAsync();
        Task<AnalysisResult> AnalyzeAsync(string sourceFragment, int updateIndex);
        Task<AnalysisResult> AnalyzeAsync(string sourceFragment);
    }
}
