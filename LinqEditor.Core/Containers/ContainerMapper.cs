using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.Containers
{
    public class ContainerMapper : IContainerMapper
    {
        private ConcurrentDictionary<string, Guid> _map = new ConcurrentDictionary<string, Guid>();
        private Guid _codeId = Guid.NewGuid();

        public Guid MapConnectionString(string connectionString)
        {
            return _map.GetOrAdd(connectionString, k => Guid.NewGuid());
        }

        public Guid CodeContainer { get { return _codeId; } }
    }
}
