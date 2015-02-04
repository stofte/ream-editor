using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace LinqEditor.Core.Models.Analysis
{
    public class CompletionEntry
    {
        public MemberKind Kind { get; set; }
        public string Value { get; set; }
    }
}
