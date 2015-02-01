using LinqEditor.Core.Schema.Helpers;
using System;

namespace LinqEditor.Core.Templates.Code
{
    public partial class Statements
    {
        public Guid NamespaceId { get; set; }
        public string SourceCode { get; set; }

        public string GeneratedNamespace
        {
            get { return NamespaceId.ToIdentifierWithPrefix("c"); }
        }
    }
}
