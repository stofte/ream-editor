using LinqEditor.Core.Schema.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.Templates.Query
{
    public partial class SqlServer 
    {
        public int NamespaceId { get; set; }
        public IEnumerable<TableSchema> Tables { get; set; }
        public string SourceCode { get; set; }
        public string GeneratedSchemaNamespace { get; set; }

        // todo duplicated from schema
        public string GeneratedQueryNamespace
        {
            get { return "q" + NamespaceId; }
        }
    }
}
