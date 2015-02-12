using LinqEditor.Core.Models.Analysis;
using LinqEditor.Core.Context;
using LinqEditor.Core.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.CodeAnalysis.Services
{
    public class AsyncTemplateCodeAnalysis : TemplateCodeAnalysis, IAsyncTemplateCodeAnalysis
    {
        public AsyncTemplateCodeAnalysis(ITemplateService templateService) 
            : base(templateService) { }

        public async Task InitializeAsync(string assemblyPath)
        {
            await Task.Run(() => Initialize(assemblyPath));
        }

        public async Task<AnalysisResult> AnalyzeAsync(string sourceFragment, int updateIndex)
        {
            return await Task.Run(() => Analyze(sourceFragment, updateIndex));
        }
    }
}
