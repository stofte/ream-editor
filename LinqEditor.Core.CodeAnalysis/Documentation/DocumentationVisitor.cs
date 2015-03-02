using LinqEditor.Core.CodeAnalysis.Helpers;
using LinqEditor.Core.CodeAnalysis.Services;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace LinqEditor.Core.CodeAnalysis.Documentation
{
    public class DocumentationVisitor : NuDoq.Visitor
    {
        public bool Matched { get; set; }
        public string Summary { get; set; }

        string _id;
        ISymbolStore _symbolStore;

        public DocumentationVisitor(string id, ISymbolStore symbolStore)
        {
            _id = id;
            _symbolStore = symbolStore;
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
                    Summary = string.Join("", sum.Elements.Select(x => x is NuDoq.See ? _symbolStore.TranslateCref(((NuDoq.See)x).Cref) : x.ToText()));
                }
                var typeParams = method.Elements.OfType<NuDoq.TypeParam>();
            }            
        }

        public override void VisitType(NuDoq.TypeDeclaration type)
        {
            if (type.Id == _id)
            {
                Debug.Assert(!Matched);
                Matched = true; 
                var sum = type.Elements.OfType<NuDoq.Summary>().FirstOrDefault();
                Summary = sum != null ? sum.ToText() : null;
                var typeParams = type.Elements.OfType<NuDoq.TypeParam>();
            }
        }
    }
}
