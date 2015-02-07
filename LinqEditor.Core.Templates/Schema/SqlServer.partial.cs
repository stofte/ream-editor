using LinqEditor.Core.Helpers;
using LinqEditor.Core.Models.Database;
using System;
using System.Collections.Generic;

namespace LinqEditor.Core.Templates.Schema
{
    public partial class SqlServer
    {
        public Guid NamespaceId { get; set; }
        public IEnumerable<TableSchema> Tables { get; set; }
        public string ConnectionString { get; set; }

        public string GeneratedSchemaNamespace
        {
            get { return NamespaceId.ToIdentifierWithPrefix(SchemaConstants.SchemaPrefix); }
        }
    }
}