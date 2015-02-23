using System;

namespace LinqEditor.Core.Backend
{
    public interface IAsyncSessionFactory
    {
        IAsyncSession Create(Guid id);
        void Release(IAsyncSession session);
    }
}
