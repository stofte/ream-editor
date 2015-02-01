using LinqEditor.Core.Schema.Models;
using System;
using System.Collections.Generic;

namespace LinqEditor.Core.Templates.Schema
{
    public partial class SqlServer
    {
        public Guid NamespaceId { get; set; }
        public IEnumerable<TableSchema> Tables { get; set; }

        public string GeneratedSchemaNamespace
        {
            get { return "s" + NamespaceId.ToString().Replace("-", ""); }
        }
    }
}