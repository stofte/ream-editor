using LinqEditor.Core.Models.Analysis;
using System.Threading.Tasks;

namespace LinqEditor.Core.CodeAnalysis.Services
{
    public interface IAsyncTemplateCodeAnalysis
    {
        Task InitializeAsync(string assemblyPath);
        bool IsReady { get; }
        Task<AnalysisResult> AnalyzeAsync(string sourceFragment, int updateIndex);
    }
}
