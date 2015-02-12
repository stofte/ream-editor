using LinqEditor.Core.Models.Analysis;
using LinqEditor.Core.Models.Editor;
using System;

namespace LinqEditor.Core.Backend
{
    public interface ISession
    {
        //InitializeResult Initialize(string connectionString);
        //InitializeResult Initialize();
        InitializeResult Initialize(Guid connectionId);
        LoadAppDomainResult LoadAppDomain();
        ExecuteResult Execute(string sourceFragment);
        AnalysisResult Analyze(string sourceFragment, int updateIndex);
        AnalysisResult Analyze(string sourceFragment);
    }
}
