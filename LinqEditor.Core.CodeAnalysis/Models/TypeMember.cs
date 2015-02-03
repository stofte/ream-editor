using System.Collections;
using System.Collections.Generic;

namespace LinqEditor.Core.CodeAnalysis.Models
{
    public class TypeMember
    {
        public bool IsStatic { get; set; }
        public string Name { get; set; }
        public AccessibilityModifier Accessibility { get; set; }
        public MemberKind Kind { get; set; }
    }
}
