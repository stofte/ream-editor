using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.CodeAnalysis.Models
{
    public class TypeInformation
    {
        public bool IsStatic { get; set; }
        public string Name { get; set; }
        public string Namespace { get; set; }
        public IEnumerable<TypeMember> Members { get; set; }
    }
}
