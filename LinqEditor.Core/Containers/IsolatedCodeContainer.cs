using System;

namespace LinqEditor.Core.Containers
{
    public class IsolatedCodeContainer : Isolated<CodeContainer>, IIsolatedCodeContainer
    {
        public IsolatedCodeContainer() : base(Guid.NewGuid()) { }
    }
}
