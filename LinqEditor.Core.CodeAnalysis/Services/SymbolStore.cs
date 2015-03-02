using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.CodeAnalysis.Services
{
    public class SymbolStore : ISymbolStore
    {
        protected List<INamedTypeSymbol> _symbols = new List<INamedTypeSymbol>();
        object _lock = new object();

        public void Record(IEnumerable<INamedTypeSymbol> symbols)
        {
            lock (_lock)
            {
                _symbols.AddRange(symbols.Where(x => _symbols.All(y => x.ToDisplayString() != y.ToDisplayString())));
            }
        }

        public string TranslateCref(string cref)
        {
            var match = _symbols.FirstOrDefault(s => s.GetDocumentationCommentId() == cref || 
                s.GetMembers().Any(m => m.GetDocumentationCommentId() == cref));
            return match != null ? match.ToDisplayString() : null;
        }
    }
}
