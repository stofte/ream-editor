using System;

namespace LinqEditor.Core.Containers
{
    public class IsolatedDatabaseContainer : Isolated<DatabaseContainer>, IIsolatedDatabaseContainer
    {
        public IsolatedDatabaseContainer() : base(Guid.NewGuid()) { }
    }
}
