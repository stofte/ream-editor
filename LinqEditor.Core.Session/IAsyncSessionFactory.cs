using System;

namespace LinqEditor.Core.Session
{
    public interface IAsyncSessionFactory
    {
        IAsyncSession Create(Guid id);
        void Release(IAsyncSession session);
    }
}
