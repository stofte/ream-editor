using LinqEditor.Core.Models.Analysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
