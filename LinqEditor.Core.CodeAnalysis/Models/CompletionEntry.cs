using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.CodeAnalysis.Models
{
    public class CompletionEntry
    {
        public IList<IMethodSymbol> Methods { get; set; }
        public string TypeName { get; set; }
        public ITypeSymbol Symbol { get; set; }
    }
}
