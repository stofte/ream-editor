using System;

namespace LinqEditor.Core.Containers
{
    public interface IIsolatedDatabaseContainer : IDisposable
    {
        Guid Id { get; }
        DatabaseContainer Value { get; }
    }
}
