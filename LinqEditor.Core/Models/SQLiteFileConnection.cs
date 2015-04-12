using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.Models
{
    public class SQLiteFileConnection : Connection
    {
        public override string ConnectionString
        {
            get
            {
                return _connectionString;
            }
            set
            {
                _connectionString = value;
            }
        }

        public override bool IsValidConnectionString
        {
            get { return _connectionString != null; } // dont bother validating file paths
        }

        public string FileName
        {
            get
            {
                return ParseConnectionStringPart(@"(data source)=([^;]*)");
            }
        }

        public string DatabaseName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(FileName))
                {
                    return string.Empty;
                }
                return Path.GetFileName(FileName);
            }
        }

        public override string ToString()
        {
            string val = _connectionString ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(DatabaseName))
            {
                var dirName = Path.GetDirectoryName(FileName);
                val = string.Format("{0} ({1})", DatabaseName, dirName);
            }
            return val;
        }
    }
}
