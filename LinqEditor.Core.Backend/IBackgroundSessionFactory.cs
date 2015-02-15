using System;

namespace LinqEditor.Core.Backend
{
    public interface IBackgroundSessionFactory
    {
        IBackgroundSession Create(Guid id);
        void Release(IBackgroundSession session);
    }
}
