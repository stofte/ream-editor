using LinqEditor.Core.Backend.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
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
