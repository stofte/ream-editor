using LinqEditor.Core.Helpers;
using LinqEditor.Core.Models.Database;
using System;
using System.Collections.Generic;

namespace LinqEditor.Core.Templates.Templates
{
    public partial class DatabaseSchema
    {
        public Guid NamespaceId { get; set; }
        public IEnumerable<TableSchema> Tables { get; set; }

        public string TypeName(Type t)
        {
            var ut = Nullable.GetUnderlyingType(t);
            return ut != null ? ut.Name + "?" : t.Name;
        }

        public string GeneratedSchemaNamespace
        {
            get { return NamespaceId.ToIdentifierWithPrefix(SchemaConstants.SchemaPrefix); }
        }
    }
}