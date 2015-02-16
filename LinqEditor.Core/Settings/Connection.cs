using LinqEditor.Core.Models.Editor;
using System;
using System.Collections.Generic;
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

        // string values from
        // https://msdn.microsoft.com/en-us/library/system.data.sqlclient.sqlconnection.connectionstring(VS.71).aspx

        public bool UsingIntegratedSecurity
        {
            get
            {
                if (Kind == ProgramType.Database && !string.IsNullOrWhiteSpace(ConnectionString))
                {
                    var s = new List<int>();
                    var part = ParseConnectionStringPart(@"(integrated security|trusted_connection)=([^;]*)");
                    return new string[] { "true", "yes", "sspi" }.Contains(part.ToLower());
                }
                return false;
            }
        }

        public string DatabaseSecurity
        {
            get
            {
                string val = string.Empty;
                if (Kind == ProgramType.Database && !string.IsNullOrWhiteSpace(ConnectionString))
                {
                    if (UsingIntegratedSecurity)
                    {
                        val = "Integrated Security";
                    }
                    else
                    {
                        return ParseConnectionStringPart(@"(user id|uid)=([^;]*)");
                    }
                }
                return val;
            }
        }

        public string DatabaseServer
        {
            get
            {
                return ParseConnectionStringPart(@"(data source|server|address|addr|network address)=([^;]*)");
            }
        }

        public string InitialCatalog
        {
            get
            {
                return ParseConnectionStringPart(@"(initial catalog|database)=([^;]*)");
            }
        }

        private string ParseConnectionStringPart(string regexInput)
        {
            string val = string.Empty;
            if (Kind == ProgramType.Database && !string.IsNullOrWhiteSpace(ConnectionString))
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

        public override string ToString()
        {
            string val = DisplayName ?? string.Empty;

            if (Kind == ProgramType.Database) 
            {
                val += string.Format("{3}{0}.{1} ({2})", DatabaseServer, InitialCatalog, DatabaseSecurity, string.IsNullOrWhiteSpace(val) ? string.Empty : " ");
            }

            return val;
        }
    }
}
