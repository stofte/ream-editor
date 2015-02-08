using LinqEditor.Core.Models.Editor;
using System.Threading.Tasks;

namespace LinqEditor.Core.Backend
{
    public interface IBackgroundSession
    {
        Task<InitializeResult> InitializeAsync(string connectionString);
        Task<ExecuteResult> ExecuteAsync(string sourceFragment);
        Task<LoadAppDomainResult> LoadAppDomainAsync();
    }
}
