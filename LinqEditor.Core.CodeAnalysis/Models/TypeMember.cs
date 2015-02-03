
using System.Collections;
using System.Collections.Generic;
namespace LinqEditor.Core.CodeAnalysis.Models
{
    public class TypeMember: IEqualityComparer<TypeMember>
    {
        public bool IsStatic { get; set; }
        public string Name { get; set; }
        public AccessibilityModifier Accessibility { get; set; }
        public MemberKind Kind { get; set; }

        public bool Equals(TypeMember x, TypeMember y)
        {
            return x.Name == y.Name;
        }

        public int GetHashCode(TypeMember obj)
        {
            return (obj.Name).GetHashCode();
        }
    }
}
