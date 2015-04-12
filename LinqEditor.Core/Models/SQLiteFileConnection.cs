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

        public override string ToString()
        {
            string val = _connectionString ?? string.Empty;
            var fileName = Path.GetFileName(_connectionString);
            var dirName = Path.GetDirectoryName(_connectionString);
            if (!string.IsNullOrWhiteSpace(fileName) && !string.IsNullOrWhiteSpace(dirName))
            {
                val = string.Format("{0} ({1})", fileName, dirName);
            }
            return val;
        }
    }
}
