using Microsoft.CodeAnalysis;
using System.Collections;
using System.Collections.Generic;

namespace LinqEditor.Core.Models.Analysis
{
    public class TypeMember
    {
        public bool IsStatic { get; set; }
        public string Name { get; set; }
        public AccessibilityModifier Accessibility { get; set; }
        public MemberKind Kind { get; set; }
        public ISymbol Symbol { get; set; }
        public string DocumentationId { get; set; }
    }
}
