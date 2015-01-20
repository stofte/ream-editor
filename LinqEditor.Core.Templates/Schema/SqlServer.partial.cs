using LinqEditor.Core.Schema.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.Templates.Schema
{
    public partial class SqlServer
    {
        public int NamespaceId { get; set; }
        public IEnumerable<TableSchema> Tables { get; set; }

        public string GeneratedSchemaNamespace
        {
            get { return "s" + NamespaceId; }
        }
    }
}