using LinqEditor.Core.CodeAnalysis.Services;
using LinqEditor.Core.Models.Analysis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace LinqEditor.Core.CodeAnalysis.Documentation
{
    public class DocumentationVisitor : NuDoq.Visitor
    {
        string _id;
        ISymbolStore _symbolStore;
        DocumentationElement _docs;

        public bool Matched { get; set; }
        public DocumentationElement Documentation { get { return _docs; } }

        public DocumentationVisitor(string id, ISymbolStore symbolStore)
        {
            _id = id;
            _symbolStore = symbolStore;
            _docs = new DocumentationElement
            {
                MethodExceptions = new List<string>(),
                MethodParameters = new List<Tuple<string, string>>(),
            };
        }

        public override void VisitMethod(NuDoq.Method method)
        {
            if (method.Id == _id)
            {
                Debug.Assert(!Matched);
                Matched = true;
                _docs = GetDocs(method.Elements);
            }
        }

        public override void VisitProperty(NuDoq.Property property)
        {
            if (property.Id == _id)
            {
                Debug.Assert(!Matched);
                Matched = true;
                _docs = GetDocs(property.Elements);
            }
        }

        public override void VisitType(NuDoq.TypeDeclaration type)
        {
            if (type.Id == _id)
            {
                Debug.Assert(!Matched);
                Matched = true;
                _docs = GetDocs(type.Elements);
            }
        }

        private DocumentationElement GetDocs(IEnumerable<NuDoq.Element> elms)
        {
            var summaryNodes = elms.OfType<NuDoq.Summary>().FirstOrDefault();
            var summary = string.Join("", summaryNodes.Elements.Select(x => x is NuDoq.See ?
                _symbolStore.TranslateCref(((NuDoq.See)x).Cref) : x.ToText()));
            var methodExns = elms.OfType<NuDoq.Exception>()
                .Select(x => _symbolStore.TranslateCref(x.Cref));
            var methodParams = elms.OfType<NuDoq.Param>()
                .Select(x => Tuple.Create(x.Name, x.ToText()));

            return new DocumentationElement
            {
                MethodExceptions = methodExns,
                MethodParameters = methodParams,
                Summary = summary.Trim() // should be no leading/trailing ws here
            };
        }
    }
}
