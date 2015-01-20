using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.Backend.Repository
{
    public interface ISession
    {
        Task Initialize();
        Task<IEnumerable<DataTable>> Execute(string sourceFragment);
    }
}
