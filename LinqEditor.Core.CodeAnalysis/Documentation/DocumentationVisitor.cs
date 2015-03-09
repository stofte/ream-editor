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
                var sum = method.Elements.OfType<NuDoq.Summary>().FirstOrDefault();
                if (sum != null)
                {
                    Debug.Assert(!Matched);
                    Matched = true;
                    _docs.Summary = string.Join(" ", sum.Elements.Select(x => x is NuDoq.See ? 
                        _symbolStore.TranslateCref(((NuDoq.See)x).Cref).Trim() : x.ToText().Trim()));
                    _docs.MethodExceptions = method.Elements.OfType<NuDoq.Exception>()
                        .Select(x => _symbolStore.TranslateCref(x.Cref));
                    _docs.MethodParameters = method.Elements.OfType<NuDoq.Param>()
                        .Select(x => Tuple.Create(x.Name, x.ToText().Trim()));
                }
            }            
        }

        public override void VisitType(NuDoq.TypeDeclaration type)
        {
            if (type.Id == _id)
            {
                Debug.Assert(!Matched);
                Matched = true; 
                var sum = type.Elements.OfType<NuDoq.Summary>().FirstOrDefault();
                _docs.Summary = sum != null ? sum.ToText().Trim() : string.Empty;
            }
        }
    }
}
