using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.CodeAnalysis.Services
{
    /// <summary>
    /// Contains symbols picked up during code analysis.
    /// </summary>
    public interface ISymbolStore
    {
        void Record(IEnumerable<INamedTypeSymbol> symbols);
        string TranslateCref(string cref);
    }
}
