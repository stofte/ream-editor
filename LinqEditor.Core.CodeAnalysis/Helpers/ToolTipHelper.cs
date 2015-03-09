using LinqEditor.Core.CodeAnalysis.Services;
using LinqEditor.Core.Models.Analysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.CodeAnalysis.Helpers
{
    public class ToolTipHelper : IToolTipHelper
    {
        SemanticModel _model;
        IDocumentationService _documentationService;

        public ToolTipHelper(SemanticModel model, IDocumentationService documentationService)
        {
            _model = model;
            _documentationService = documentationService;
        }

        public ToolTipData GetToolTip(int index)
        {
            ToolTipData tooltip = new ToolTipData
            {
                Addendums = new List<string>()
            };
            var tree = _model.SyntaxTree.GetRoot();
            var charSpan = new TextSpan(index, 1);
            var nodes = tree.DescendantNodes(charSpan);

            var varNode = nodes.OfType<VariableDeclarationSyntax>().FirstOrDefault();
            var preNode = nodes.OfType<PredefinedTypeSyntax>().FirstOrDefault();
            var idNode = nodes.OfType<IdentifierNameSyntax>().FirstOrDefault();
            var ctorNode = nodes.OfType<ObjectCreationExpressionSyntax>().FirstOrDefault();
            var genericNodes = nodes.OfType<GenericNameSyntax>();
            GenericNameSyntax genericNode = null;
            foreach (GenericNameSyntax node in nodes.OfType<GenericNameSyntax>())
            {
                if (node.Span.Start <= index && index <= node.Span.End)
                {
                    if (genericNode == null)
                    {
                        genericNode = node;
                    }
                    else if (genericNode.Span.Start <= node.Span.Start && node.Span.End <= genericNode.Span.End)
                    {
                        // check if node is more specific (sub span of previous matches, if so, prefer this
                        genericNode = node;
                    }
                }
            }


            // check for the types of nods we want to show tooltip for:
            // a. hovering over a new expression
            var isConstructor = ctorNode != null;
            // b. hover over the type decl of an assignment statement
            var isDeclaration = varNode != null && preNode != null && preNode.Span.Start == varNode.SpanStart ||
                idNode != null && idNode.SpanStart == varNode.SpanStart;

            var isGeneric = genericNode != null;

            // if hovering over a sub element of a constructor expression (eg over the type specifier)
            // we show the same style tooltip as for decl nodes
            if (isDeclaration || isConstructor && preNode != null || isConstructor && isGeneric)
            {
                var t = 
                    isDeclaration ? _model.GetTypeInfo(varNode.Type).Type :
                    isConstructor && preNode != null ? _model.GetSymbolInfo(preNode).Symbol as ITypeSymbol :
                    _model.GetSymbolInfo(genericNode).Symbol as ITypeSymbol;
                 var docMemberId = t.OriginalDefinition != null && t != t.OriginalDefinition ?
                    t.OriginalDefinition.GetDocumentationCommentId() : t.GetDocumentationCommentId();
                var docs = _documentationService.GetDocumentation(docMemberId);
                var nameAndTypes = GetDisplayNameAndSpecializations(t);

                tooltip.ItemName = nameAndTypes.Item1;
                tooltip.Addendums = nameAndTypes.Item2;
                tooltip.Description = docs != null ? docs.Summary : string.Empty;
            }
            else if (isConstructor)
            {
                var symInfo = _model.GetSymbolInfo(ctorNode.Type);
                // obtains symbols of any passed arguments
                var argSymbols = ctorNode.ArgumentList.Arguments.Select(x => _model.GetTypeInfo(x.Expression));
                // match the ctor based on the arguments
                var namedSymbol = symInfo.Symbol as INamedTypeSymbol;
                var calledCtor = namedSymbol
                    .Constructors
                    .FirstOrDefault(x =>
                        // if the count matches
                        x.Parameters.Count() == argSymbols.Count() &&
                        x.Parameters
                            // select into the passed args and check that either the type is a 
                            // direct match, or that it implements the required interface
                            .Select((p, i) =>
                                p.Type == argSymbols.ElementAt(i).Type ||
                                argSymbols.ElementAt(i).Type.AllInterfaces.Contains(p.Type))
                            // all the params must match
                            .All(b => b == true));

                if (calledCtor != null)
                {
                    // resolve the original definition, if the ctor is generic
                    var t = calledCtor.OriginalDefinition != null && calledCtor != calledCtor.OriginalDefinition ? calledCtor.OriginalDefinition : calledCtor;
                    var docMemberId = t.GetDocumentationCommentId();
                    var docs = _documentationService.GetDocumentation(docMemberId);

                    var typeName = calledCtor.ContainingType.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
                    var typeNs = calledCtor.ContainingType.ContainingNamespace.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
                    var shortTypeName = typeName.Substring(typeNs.Length + 1);
                    var className = shortTypeName.IndexOf("<") > -1 ?
                        shortTypeName.Substring(0, shortTypeName.IndexOf("<")) : shortTypeName;
                    var argStrings = calledCtor.Parameters.Select((x, i) =>
                        {
                        var ns = x.Type.ContainingNamespace.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
                        var full = x.Type.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
                        // suffix parameter name from docs
                        return string.Format("{0} {1}", full.Substring(ns.Length + 1), docs.MethodParameters.ElementAt(i).Item1);
                    });
                    var availableCtors = namedSymbol.Constructors.Where(x => x.DeclaredAccessibility == Accessibility.Public);
                    var ctorCount = availableCtors.Count() > 1 ? string.Format("(+ {0} overload(s))", availableCtors.Count() - 1) : string.Empty;

                    tooltip.ItemName = string.Format("{0}.{1}({2}){3}{4}", shortTypeName, className, string.Join(", ", argStrings),
                        !string.IsNullOrWhiteSpace(ctorCount) ? " " : string.Empty, ctorCount);
                    tooltip.Description = docs != null ? docs.Summary : string.Empty;
                    var addendums = new List<string>();

                    if (docs.MethodExceptions.Count() > 0)
                    {
                        addendums.Add("Exceptions:");
                        addendums.AddRange(docs.MethodExceptions.Select(x => string.Format("\t{0}", x)));
                    }

                    tooltip.Addendums = addendums;
                }
            }

            return tooltip;
        }

        Tuple<string, IEnumerable<string>> GetDisplayNameAndSpecializations(ITypeSymbol t)
        {
            //.OriginalDefinition != null && type.Type != type.Type.OriginalDefinition ?
            //type.Type.OriginalDefinition : type.Type;

            var specializations = new List<string>();

            var kind = t.IsValueType ? "struct" :
                        t.IsReferenceType && t.TypeKind == TypeKind.Interface ? "interface" :
                        t.IsReferenceType ? "class" :
                        t.IsNamespace ? "namespace" : string.Empty;

            var name = string.Join("", t.ToDisplayParts(SymbolDisplayFormat.CSharpErrorMessageFormat));

            if (!t.IsNamespace && !name.Contains(".")) // namespaces can be top level
            {
                // for type aliases ToDisplay returns the alias name
                // for tooltips, we want the original full type name
                name = GetBasicName(t);
            }

            // if the type is generic, assume it's a namedtype
            if (name.Contains("<") && t is INamedTypeSymbol)
            {
                var namedT = t as INamedTypeSymbol;

                // this assumes the collections are sorted in the same order
                specializations.AddRange(namedT.TypeParameters.Select((x, i) => string.Format("{0} is {1}", x, GetBasicName(namedT.TypeArguments[i]))));

                // attempt to construct a new generic string with specific variance, eg "<out T>"
                string varianceStr = string.Empty;
                if (namedT != null)
                {
                    varianceStr = string.Format("<{0}>",
                        string.Join(",", namedT.TypeParameters.Select((x, i) =>
                            string.Format("{0}{1}",
                                x.Variance != VarianceKind.None ? x.Variance.ToString().ToLower() + " " : string.Empty,
                                x.Name))));
                }

                if (!string.IsNullOrWhiteSpace(varianceStr))
                {
                    var firstIdx = name.IndexOf("<");
                    var length = name.LastIndexOf(">") - firstIdx;
                    name = name.Replace(name.Substring(firstIdx, length + 1), varianceStr);
                }
            }

            return Tuple.Create(string.Format("{0} {1}", kind, name), specializations.AsEnumerable());
        }

        string GetBasicName(ITypeSymbol t)
        {
            var nss = new List<string>();
            nss.Add(t.Name); // todo: generics?
            var ns = t.ContainingNamespace;
            do
            {
                if (!string.IsNullOrWhiteSpace(ns.Name))
                {
                    nss.Add(ns.Name);
                }
                ns = ns.ContainingNamespace;
            }
            while (ns != null);
            nss.Reverse();
            var baseName = string.Join(".", nss);

            var namedType = t as INamedTypeSymbol;
            if (namedType != null && namedType.TypeArguments.Count() > 0)
            {
                baseName += string.Format("<{0}>", string.Join(",", namedType.TypeArguments.Select(x => GetBasicName(x))));
            }
            return baseName;
        }
    }
}
