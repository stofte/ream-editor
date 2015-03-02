using LinqEditor.Core.CodeAnalysis.Compiler;
using LinqEditor.Core.CodeAnalysis.Documentation;
using LinqEditor.Core.Models.Analysis;
using Microsoft.CodeAnalysis;
using NuDoq;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Xml.Linq;

namespace LinqEditor.Core.CodeAnalysis.Services
{
    public class DocumentationService : IDocumentationService
    {
        ISymbolStore _symbolStore;

        IEnumerable<DocumentMembers> _documents;

        IEnumerable<CustomDocumentationProvider> _providers;

        public DocumentationService(ISymbolStore symbolStore)
        {
            _symbolStore = symbolStore;
            var list = new List<CustomDocumentationProvider>();
            var list2 = new List<DocumentMembers>();
            foreach (var assembly in CSharpCompiler.GetCoreAssemblies())
            {
                var path = CustomDocumentationProvider.GetAssemblyDocmentationPath(assembly);
                list.Add(new CustomDocumentationProvider(assembly));
                list2.Add(NuDoq.DocReader.Read(path));
            }
            _providers = list;
            _documents = list2;
        }
        
        public DocumentationElement GetDocumentation(string documentationId)
        {
            DocumentationElement elm = null;
            foreach (var doc in _documents)
            {
                var res = doc.Accept(new DocumentationVisitor(documentationId, _symbolStore));
                if (res.Matched)
                {
                    elm = new DocumentationElement
                    {
                        Id = documentationId,
                        Summary = res.Summary
                    };
                    break;
                }
            }
            return elm;
        }
    }
}
