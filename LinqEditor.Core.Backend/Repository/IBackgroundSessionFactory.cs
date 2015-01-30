using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.Backend.Repository
{
    public interface IBackgroundSessionFactory
    {
        IBackgroundSession Create(string connectionString);
        void Release(IBackgroundSession backgroundSession);
    }
}
