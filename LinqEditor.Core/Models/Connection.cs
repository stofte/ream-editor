using LinqEditor.Core.Models.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.Models
{
    [Serializable]
    public abstract class Connection
    {
        protected string _connectionString;

        public Guid Id { get; set; }
        public string DisplayName { get; set; }

        // todo: this should not be here, mapping should exists inside service itself
        public string CachedSchemaFileName { get; set; }
        public string CachedSchemaNamespace { get; set; }

        public abstract string ConnectionString { get; set; }
        public abstract bool IsValidConnectionString { get; }
        public abstract override string ToString();
    }
}
