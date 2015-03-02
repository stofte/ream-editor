using LinqEditor.Core.Models.Analysis;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Xml.Linq;

namespace LinqEditor.Core.CodeAnalysis.Services
{
    public interface IDocumentationService
    {
        XElement GetDocumentation(string memberName);
        DocumentationElement GetDocs(string documentationId, IEnumerable<INamedTypeSymbol> availableSymbols);
    }
}
