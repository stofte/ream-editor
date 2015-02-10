using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.Settings
{
    [Serializable]
    public class Connection
    {
        public Guid Id { get; set; }
        public string ConnectionString { get; set; }
        public string DisplayName { get; set; }
        public string CachedSchemaFileName { get; set; }
        public string CachedSchemaNamespace { get; set; }
    }
}
