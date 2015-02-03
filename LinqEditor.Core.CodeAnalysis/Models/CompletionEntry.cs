using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace LinqEditor.Core.CodeAnalysis.Models
{
    public class CompletionEntry
    {
        public MemberKind Kind { get; set; }
        public string Value { get; set; }
    }
}
