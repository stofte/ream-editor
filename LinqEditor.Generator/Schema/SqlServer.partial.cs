using LinqEditor.Generator.Interfaces;
using LinqEditor.Schema.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Generator.Schema
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