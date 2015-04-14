using LinqEditor.Core.Models.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LinqEditor.Core.Models
{
    [Serializable]
    public abstract class Connection
    {
        protected string _connectionString;

        public Guid Id { get; set; }
        public string DisplayName { get; set; }

        public string SchemaHash { get; set; }

        // todo: this should not be here, mapping should exists inside service itself
        public string CachedSchemaFileName { get; set; }
        public string CachedSchemaNamespace { get; set; }

        protected string ParseConnectionStringPart(string regexInput)
        {
            string val = string.Empty;
            if (!string.IsNullOrWhiteSpace(ConnectionString))
            {
                var regex = new Regex(regexInput, RegexOptions.IgnoreCase);
                var m = regex.Match(ConnectionString);
                if (m.Success)
                {
                    val = m.Groups[2].Value.Trim();
                }
            }
            return val;
        }

        public abstract string ConnectionString { get; set; }
        public abstract bool IsValidConnectionString { get; }
        public abstract override string ToString();
    }
}
