using LinqEditor.Generator.Interfaces;
using LinqEditor.Schema.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Generator.Query
{
    public partial class SqlServer : IQueryGenerator
    {
        public int NamespaceId { get; set; }
        public IEnumerable<TableSchema> Tables { get; set; }
        public string SourceCode { get; set; }
        public string GeneratedSchemaNamespace { get; set; }

        // todo duplicated from schema
        public string GeneratedQueryNamespace
        {
            get { return "LinqEditorGeneratedQuery" + NamespaceId; }
        }
    }
}
