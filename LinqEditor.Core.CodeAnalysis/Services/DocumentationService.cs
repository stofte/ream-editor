using LinqEditor.Core.CodeAnalysis.Compiler;
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
        IEnumerable<CustomDocumentationProvider> _providers;

        public DocumentationService()
        {
            var list = new List<CustomDocumentationProvider>();
            foreach (var assembly in CSharpCompiler.GetCoreAssemblies())
            {
                list.Add(new CustomDocumentationProvider(assembly));
            }
            _providers = list;
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
    }
}
