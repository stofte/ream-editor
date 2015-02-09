using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.Settings
{
    [Serializable]
    public class Connection : IEqualityComparer<Connection>
    {
        public string ConnectionString { get; set; }
        public string DisplayName { get; set; }
        public string CachedSchemaFileName { get; set; }
        public string CachedSchemaNamespace { get; set; }

        public bool Equals(Connection x, Connection y)
        {
            return x.ConnectionString == y.ConnectionString;
        }

        public int GetHashCode(Connection obj)
        {
            return obj.ConnectionString.GetHashCode();
        }
    }
}
