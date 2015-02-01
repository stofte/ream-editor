using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqEditor.Core.Schema.Helpers;

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
