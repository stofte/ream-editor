using System;
using System.Collections.Generic;

namespace LinqEditor.Core.Settings
{
    public interface IConnectionStore
    {
        void Add(Connection conn);
        void Update(Connection conn);
        void Delete(Connection conn);
        IEnumerable<Connection> Connections { get; }
        event Action ConnectionsUpdated;
    }
}
