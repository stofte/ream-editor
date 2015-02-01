using LinqEditor.Core.Backend.Models;

namespace LinqEditor.Core.Backend.Repository
{
    public interface ISession
    {
        InitializeResult Initialize(string connectionString);
        LoadAppDomainResult LoadAppDomain();
        ExecuteResult Execute(string sourceFragment);
    }
}
