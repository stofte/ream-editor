using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace LinqEditor.Core.CodeAnalysis.Models
{
    public class CompletionEntry
    {
        public IList<IMethodSymbol> Methods { get; set; }
        public string TypeName { get; set; }
        public ITypeSymbol Symbol { get; set; }
    }
}
