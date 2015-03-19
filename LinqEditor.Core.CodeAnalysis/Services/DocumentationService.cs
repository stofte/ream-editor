using LinqEditor.Core.CodeAnalysis.Compiler;
using LinqEditor.Core.CodeAnalysis.Documentation;
using LinqEditor.Core.Models.Analysis;
using Microsoft.CodeAnalysis;
using NuDoq;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Xml.Linq;

namespace LinqEditor.Core.CodeAnalysis.Services
{
    public class DocumentationService : IDocumentationService
    {
        ISymbolStore _symbolStore;
        IEnumerable<DocumentMembers> _documents;
        
        public DocumentationService(ISymbolStore symbolStore)
        {
            _symbolStore = symbolStore;
            var list = new List<DocumentMembers>();
            foreach (var assembly in CSharpCompiler.GetCoreAssemblies())
            {
                var n = assembly.GetName().Name;
                var filePath = string.Format("{1}{0}.xml", assembly.GetName().Name, PathUtility.ApplicationDirectory);
                list.Add(NuDoq.DocReader.Read(filePath));
            }
            _documents = list;
        }
        
        public DocumentationElement GetDocumentation(string documentationId)
        {
            DocumentationElement elm = null;
            foreach (var doc in _documents)
            {
                var res = doc.Accept(new DocumentationVisitor(documentationId, _symbolStore));
                if (res.Matched)
                {
                    elm = res.Documentation;
                    break;
                }
            }
            return elm;
        }
    }
}
