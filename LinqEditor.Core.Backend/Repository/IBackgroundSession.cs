using LinqEditor.Core.Backend.Models;
using System.Threading.Tasks;

namespace LinqEditor.Core.Backend.Repository
{
    public interface IBackgroundSession
    {
        Task<InitializeResult> InitializeAsync(string connectionString);
        Task<ExecuteResult> ExecuteAsync(string sourceFragment);
        Task<LoadAppDomainResult> LoadAppDomainAsync();
    }
}
