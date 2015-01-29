using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
