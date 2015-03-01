using LinqEditor.Core.CodeAnalysis.Compiler;
using LinqEditor.Core.CodeAnalysis.Documentation;
using LinqEditor.Core.Models.Analysis;
using NuDoq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LinqEditor.Core.CodeAnalysis.Services
{
    public class DocumentationService : IDocumentationService
    {
        IEnumerable<DocumentMembers> _documents;

        IEnumerable<CustomDocumentationProvider> _providers;

        public DocumentationService()
        {
            var list = new List<CustomDocumentationProvider>();
            var list2 = new List<DocumentMembers>();
            foreach (var assembly in CSharpCompiler.GetCoreAssemblies())
            {
                //var path = CustomDocumentationProvider.GetAssemblyDocmentationPath(assembly);
                list.Add(new CustomDocumentationProvider(assembly));
                //list2.Add(NuDoq.DocReader.Read(path));
            }
            _providers = list;
            _documents = list2;
        }

        public XElement GetDocumentation(string memberName)
        {
            foreach (var provider in _providers)
            {
                var result = provider.GetDocumentation(memberName);
                if (!string.IsNullOrWhiteSpace(result))
                {
                    return XElement.Parse(result);
                }
            }
            return null;
        }

        public DocumentationElement GetDocs(string memberName)
        {
            foreach (var doc in _documents)
            {
                var result = doc.Accept(new DocumentationVisitor(memberName));
                
            }
            return null;
        }
    }
}
