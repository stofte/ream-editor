using LinqEditor.Core.Backend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.Backend.Repository
{
    public interface ISession
    {
        InitializeResult Initialize(string connectionString);
        LoadAppDomainResult LoadAppDomain();
        ExecuteResult Execute(string sourceFragment);
    }
}
