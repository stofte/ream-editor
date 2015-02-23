using LinqEditor.Core.Models.Analysis;
using LinqEditor.Core.Models.Editor;
using System;
using System.Threading;

namespace LinqEditor.Core.Session
{
    public interface ISession
    {
        InitializeResult Initialize(Guid connectionId);
        InitializeResult Reinitialize();
        LoadAppDomainResult LoadAppDomain();
        ExecuteResult Execute(string sourceFragment);
        ExecuteResult Execute(string sourceFragment, CancellationToken ct);
        AnalysisResult Analyze(string sourceFragment, int updateIndex);
        AnalysisResult Analyze(string sourceFragment);
    }
}
