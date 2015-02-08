using LinqEditor.Core.Models.Editor;

namespace LinqEditor.Core.Backend
{
    public interface ISession
    {
        InitializeResult Initialize(string connectionString);
        InitializeResult Initialize();
        LoadAppDomainResult LoadAppDomain();
        ExecuteResult Execute(string sourceFragment);
    }
}
