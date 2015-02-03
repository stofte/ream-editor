using LinqEditor.Core.CodeAnalysis.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.CodeAnalysis.Services
{
    public interface IAsyncTemplateCodeAnalysis
    {
        bool IsReady { get; }
        Task<AnalysisResult> AnalyzeAsync(string sourceFragment, int updateIndex);
    }
}
