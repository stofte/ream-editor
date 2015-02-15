using LinqEditor.Core.Models.Editor;
using System;
using System.Linq;
using System.Text.RegularExpressions;

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
            if (!string.IsNullOrWhiteSpace(DisplayName))
            {
                return DisplayName;
            }

            if (!string.IsNullOrWhiteSpace(ConnectionString))
            {
                var db = new Regex(@"data source=([^;]*)", RegexOptions.IgnoreCase);
                var catalog = new Regex(@"initial catalog=([^;]*)", RegexOptions.IgnoreCase);
                var integratedSecurity = new Regex(@"integrated security=([^;]*)", RegexOptions.IgnoreCase);
                var dbMatch = db.Match(ConnectionString);
                var catalogMatch = catalog.Match(ConnectionString);
                var integratedSecurityMatch = integratedSecurity.Match(ConnectionString);
                if (dbMatch.Success && catalogMatch.Success)
                {
                    // integrated security values
                    // https://msdn.microsoft.com/en-us/library/system.data.sqlclient.sqlconnection.connectionstring(VS.71).aspx
                    var isIntegratedSecurity = new string[] { "true", "yes", "sspi" }.Contains(integratedSecurityMatch.Groups[1].Value.Trim().ToLower()); 
                    var suffix = integratedSecurityMatch.Success ? " (Integrated Security)" : string.Empty;
                    return string.Format("{0}.{1}{2}", dbMatch.Groups[1].Value, catalogMatch.Groups[1].Value, suffix);
                }
                return string.Format("{0} ({1})", DisplayName, ConnectionString);
            }
            return DisplayName;
        }
    }
}
