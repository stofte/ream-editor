using System.Collections.Generic;

namespace LinqEditor.Core.CodeAnalysis.Models
{
    /// <summary>
    /// Contains type information for available types
    /// </summary>
    public class TypeInformation
    {
        public bool IsStatic { get; set; }
        public string Name { get; set; }
        public string Namespace { get; set; }
        public IEnumerable<TypeMember> Members { get; set; }
    }
}
