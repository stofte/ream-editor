using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.Backend
{
    public interface IBackgroundSessionFactory
    {
        IBackgroundSession Create(Guid id);
        void Release(IBackgroundSession session);
    }
}
