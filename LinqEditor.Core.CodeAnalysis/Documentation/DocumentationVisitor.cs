using LinqEditor.Core.CodeAnalysis.Helpers;
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
        IEnumerable<INamedTypeSymbol> _availableSymbols;

        public DocumentationVisitor(string id, IEnumerable<INamedTypeSymbol> availableSymbols)
        {
            _id = id;
            _availableSymbols = availableSymbols;
        }

        private string TranslateCref(NuDoq.See see)
        {
            var symbol = _availableSymbols.FirstOrDefault(x => 
                x.GetDocumentationCommentId() == see.Cref ||
                x.GetMembers().Any(m => m.GetDocumentationCommentId() == see.Cref));
            var symbolName = CodeAnalysisHelper.GetBasicName(symbol);
            return symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
        }

        public override void VisitMethod(NuDoq.Method method)
        {
            if (method.Id == _id)
            {
                Debug.Assert(!Matched);
                var sym = _availableSymbols.FirstOrDefault(x => x.GetMembers().Any(m => m.GetDocumentationCommentId() == _id));
                Matched = true;
                var sum = method.Elements.OfType<NuDoq.Summary>().FirstOrDefault();
                if (sum != null)
                {
                    Summary = string.Join("", sum.Elements.Select(x => x is NuDoq.See ? TranslateCref(x as NuDoq.See) : x.ToText()));
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
