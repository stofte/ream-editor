using LinqEditor.Core.Models.Editor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace LinqEditor.Core.Settings
{
    [Serializable]
    public class Connection
    {
        private string _connectionString;

        public Connection()
        {
            Kind = ProgramType.Database;
            _connectionString = string.Empty;
        }

        public Guid Id { get; set; }

        public string DisplayName { get; set; }
        public string CachedSchemaFileName { get; set; }
        public string CachedSchemaNamespace { get; set; }
        public ProgramType Kind { get; set; }

        public string ConnectionString
        {
            get
            {
                return _connectionString;
            }
            set
            {
                // not so nice to throw from a property
                new System.Data.SqlClient.SqlConnection(value);
                _connectionString = value;
            }
        }

        // string values from
        // https://msdn.microsoft.com/en-us/library/system.data.sqlclient.sqlconnection.connectionstring(VS.71).aspx
        [JsonIgnore]
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

        [JsonIgnore]
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

        [JsonIgnore]
        public string DatabaseServer
        {
            get
            {
                return ParseConnectionStringPart(@"(data source|server|address|addr|network address)=([^;]*)");
            }
        }

        [JsonIgnore]
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

        [JsonIgnore]
        public bool IsValidConnectionString
        {
            get
            {
                return DatabaseServer.Length > 0 &&
                    InitialCatalog.Length > 0 &&
                    (UsingIntegratedSecurity ||
                    !UsingIntegratedSecurity && DatabaseSecurity.Length > 0);
            }
        }

        public override string ToString()
        {
            string val = DisplayName ?? string.Empty;

            if (Kind == ProgramType.Database) 
            {
                if (IsValidConnectionString)
                {
                    val += string.Format("{3}{0}.{1} ({2})", DatabaseServer, InitialCatalog, DatabaseSecurity, string.IsNullOrWhiteSpace(val) ? string.Empty : " ");
                }
                else
                {
                    val += ConnectionString;
                }
            }

            return val;
        }
    }
}
