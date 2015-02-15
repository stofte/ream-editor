using LinqEditor.Core.Models.Editor;
using System;

namespace LinqEditor.Core.Settings
{
    [Serializable]
    public class Connection
    {
        public Connection()
        {
            Kind = ProgramType.Database;
        }

        public Guid Id { get; set; }
        public string ConnectionString { get; set; }
        public string DisplayName { get; set; }
        public string CachedSchemaFileName { get; set; }
        public string CachedSchemaNamespace { get; set; }
        public ProgramType Kind { get; set; }

        public override string ToString()
        {
            if (!string.IsNullOrWhiteSpace(ConnectionString))
            {
                return string.Format("{0} ({1})", DisplayName, ConnectionString);
            }
            return DisplayName;
        }
    }
}
