using System.Collections.Generic;

namespace LinqEditor.Core.Models.Analysis
{
    /// <summary>
    /// Contains all type information for the type
    /// </summary>
    public class TypeInformation
    {
        public bool IsStatic { get; set; }
        public string Name { get; set; }
        public string Namespace { get; set; }
        public IEnumerable<TypeMember> Members { get; set; }
    }
}
