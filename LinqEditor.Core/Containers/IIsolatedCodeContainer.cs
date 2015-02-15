using System;

namespace LinqEditor.Core.Containers
{
    public interface IIsolatedCodeContainer : IDisposable
    {
        Guid Id { get; }
        CodeContainer Value { get; }
    }
}
