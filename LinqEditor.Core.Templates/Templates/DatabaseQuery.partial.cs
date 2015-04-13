using LinqEditor.Core.Helpers;
using LinqEditor.Core.Models.Database;
using System;
using System.Collections.Generic;

namespace LinqEditor.Core.Templates.Templates
{
    public partial class DatabaseQuery 
    {
        public Guid NamespaceId { get; set; }
        public IEnumerable<TableSchema> Tables { get; set; }
        public string SourceCode { get; set; }
        public string GeneratedSchemaNamespace { get; set; }

        // todo duplicated from schema
        public string GeneratedQueryNamespace
        {
            get { return NamespaceId.ToIdentifierWithPrefix(SchemaConstants.QueryPrefix); }
        }
    }
}
