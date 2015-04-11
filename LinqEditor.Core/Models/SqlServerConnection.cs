using LinqEditor.Core.Models.Editor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LinqEditor.Core.Models
{
    public class SqlServerConnection : Connection
    {
        public override string ConnectionString
        {
            get
            {
                return _connectionString;
            }
            set
            {
                // dirty syntax validation
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
                if (!string.IsNullOrWhiteSpace(ConnectionString))
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
                if (!string.IsNullOrWhiteSpace(ConnectionString))
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
        public string DatabasePassword
        {
            get
            {
                if (!UsingIntegratedSecurity)
                {
                    return ParseConnectionStringPart(@"(password|pwd)=([^;]*)");
                }   
                return string.Empty;
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

        [JsonIgnore]
        public override bool IsValidConnectionString
        {
            get
            {
                return DatabaseServer.Length > 0 &&
                    InitialCatalog.Length > 0 &&
                    (UsingIntegratedSecurity ||
                    !UsingIntegratedSecurity && DatabaseSecurity.Length > 0);
            }
        }

        /// <summary>
        /// Returns suitable connection strings for the current connection, modified for the passed database names.
        /// </summary>
        public IEnumerable<string> GetConnectionStringWithDatabases(IEnumerable<string> databaseNames)
        {
            var conn = new List<string>();
            foreach(var name in databaseNames) 
            {
                var connStr = string.Format("server={0};database={1};", DatabaseServer, name);
                if (UsingIntegratedSecurity)
                {
                    connStr += "integrated security=sspi;";
                }
                else
                {
                    connStr += string.Format("user id={0};pwd={1};", DatabaseSecurity, DatabasePassword);
                }

                conn.Add(connStr);
            }
            return conn;
        }

        public override string ToString()
        {
            string val = string.IsNullOrWhiteSpace(DisplayName) ? string.Empty : DisplayName;

            if (IsValidConnectionString)
            {
                val += string.Format("{3}{0}.{1} ({2})", DatabaseServer, InitialCatalog, DatabaseSecurity, string.IsNullOrWhiteSpace(val) ? string.Empty : " ");
            }
            else
            {
                val += ConnectionString;
            }
            
            return val;
        }

        private string ParseConnectionStringPart(string regexInput)
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
    }
}
