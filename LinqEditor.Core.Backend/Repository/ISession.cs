using LinqEditor.Core.Models.Editor;

namespace LinqEditor.Core.Backend.Repository
{
    public interface ISession
    {
        InitializeResult Initialize(string connectionString);
        LoadAppDomainResult LoadAppDomain();
        ExecuteResult Execute(string sourceFragment);
    }
}
