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
            var addendums = new List<string>();

            ToolTipData tooltip = new ToolTipData
            {
                Addendums = new List<string>()
            };

            DocumentationElement docElm = null;
            ITypeSymbol nodeType = null;
            IPropertySymbol nodeProp = null;
            SyntaxNode matchedNode = GetIndexNode(index);

            // get initial type of the node
            if (matchedNode is IdentifierNameSyntax || 
                matchedNode is ObjectCreationExpressionSyntax)
            {
                // vars, direct references, aliases
                nodeType = _model.GetTypeInfo(matchedNode).Type;
            }
            else if (matchedNode is PredefinedTypeSyntax || 
                     matchedNode is GenericNameSyntax)
            {
                nodeType = _model.GetSymbolInfo(matchedNode).Symbol as ITypeSymbol;
            }
            else if (matchedNode is MemberAccessExpressionSyntax)
            {
                nodeProp = _model.GetSymbolInfo(matchedNode).Symbol as IPropertySymbol;
            }

            // return if nothing was found
            if (nodeType == null && nodeProp == null) return tooltip;

            if (nodeType != null &&
                (matchedNode is IdentifierNameSyntax || 
                matchedNode is PredefinedTypeSyntax ||
                matchedNode is GenericNameSyntax)) // type references
            {
                var docId = nodeType.OriginalDefinition != null && nodeType != nodeType.OriginalDefinition ?
                    nodeType.OriginalDefinition.GetDocumentationCommentId() : nodeType.GetDocumentationCommentId();
                docElm = _documentationService.GetDocumentation(docId);
                // use standard display routine
                var typeParts = TypeDisplayStrings(nodeType);
                var nameAndTypes = TypeDisplayStrings(nodeType);
                tooltip.ItemName = nameAndTypes.Item1;
                addendums.AddRange(nameAndTypes.Item2);
            }
            else if (matchedNode is MemberAccessExpressionSyntax && nodeProp != null)
            {
                if (nodeProp.Kind == SymbolKind.Property)
                {
                    var docId = nodeProp.GetDocumentationCommentId();
                    docElm = _documentationService.GetDocumentation(docId);

                    tooltip.ItemName = string.Format("{0} {1}.{2}",
                        nodeProp.Type.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
                        nodeProp.ContainingType.Name,
                        nodeProp.Name);
                    addendums.AddRange(MapExtras(docElm));
                }
            }
            else if (matchedNode is ObjectCreationExpressionSyntax && 
                nodeType is INamedTypeSymbol) // constructor
            {
                var ctorNode = matchedNode as ObjectCreationExpressionSyntax;
                var classType = nodeType as INamedTypeSymbol;
                var argSymbols = ctorNode.ArgumentList.Arguments.Select(x => _model.GetTypeInfo(x.Expression));
                // only search for public ctors
                var calledCtor = classType.Constructors
                    .Where(x => x.DeclaredAccessibility == Accessibility.Public)
                    .FirstOrDefault(x =>
                        // if the count matches
                        x.Parameters.Count() == argSymbols.Count() &&
                        x.Parameters
                            // select into the passed args and check that either the type is a 
                            // direct match, or that it implements the required interface
                            .Select((p, i) =>
                                p.Type == argSymbols.ElementAt(i).Type ||
                                argSymbols.ElementAt(i).Type.AllInterfaces.Contains(p.Type))
                            .All(b => b == true));

                // if we found a matching ctor
                if (calledCtor != null)
                {
                    var docId = calledCtor.OriginalDefinition != null && calledCtor != calledCtor.OriginalDefinition ?
                        calledCtor.OriginalDefinition.GetDocumentationCommentId() : calledCtor.GetDocumentationCommentId();
                    docElm = _documentationService.GetDocumentation(docId);
                    tooltip.ItemName = MethodDisplayStrings(calledCtor, docElm);
                    addendums.AddRange(MapExtras(docElm));
                }
            }

            tooltip.Description = docElm != null ? docElm.Summary : string.Empty;
            tooltip.Addendums = addendums;
            return tooltip;
        }

        /// <summary>
        /// Returns the most relevant syntax node for the given index.
        /// </summary>
        SyntaxNode GetIndexNode(int index)
        {
            var tree = _model.SyntaxTree.GetRoot();
            SyntaxNode match = null;
            foreach (var node in tree.DescendantNodes(new TextSpan(index, 1)))
            {
                if (match == null)
                {
                    match = node;
                    continue;
                }

                // if we already have a member access node, dont replace it with a identifier node, since that the access nodeis more useful
                if (match is MemberAccessExpressionSyntax && node is IdentifierNameSyntax)
                {
                    continue;
                }

                var withIn = node.SpanStart <= index && index <= node.Span.End && // within span
                    (match.SpanStart < node.SpanStart || node.Span.End < match.Span.End); // and either index is closer

                if (withIn)
                {
                    match = node;
                }
            }
            return match;
        }

        /// <summary>
        /// Returns a formatted list of strings of documentation extras (exceptions, etc)
        /// </summary>
        IEnumerable<string> MapExtras(DocumentationElement docs)
        {
            var l = new List<string>();

            if (docs.MethodExceptions.Count() > 0)
            {
                l.Add("Exceptions:");
                l.AddRange(docs.MethodExceptions.Select(x => string.Format("\t{0}", x)));
            }

            return l;
        }

        /// <summary>
        /// Renders the methodsymbol in a VS style, showing number of overloads available,
        /// and for regular methods, return types.
        /// </summary>
        string MethodDisplayStrings(IMethodSymbol m, DocumentationElement docElm)
        {
            if (m.MethodKind == MethodKind.Constructor)
            {
                var availableCtors = m.ContainingType.Constructors.Where(x => x.DeclaredAccessibility == Accessibility.Public);
                var ctorCount = availableCtors.Count() > 1 ? string.Format(" (+ {0} overload(s))", availableCtors.Count() - 1) : string.Empty;
                var argStrings = m.Parameters.Select((x, i) =>
                {
                    var ns = x.Type.ContainingNamespace.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
                    var full = x.Type.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
                    // suffix parameter name from docs
                    return string.Format("{0} {1}", full.Substring(ns.Length + 1), docElm.MethodParameters.ElementAt(i).Item1);
                });

                var fullNs = m.ContainingNamespace.ToDisplayString();
                var ctorName = m.ToDisplayString().Substring(fullNs.Length + 1);
                var ctorNameStub = "." + m.ContainingType.Name + "(";
                ctorName = ctorName.Substring(0, ctorName.IndexOf(ctorNameStub) + ctorNameStub.Length - 1);

                return string.Format("{0}({1}){2}", ctorName, string.Join(", ", argStrings), ctorCount);
            }

            return string.Empty;
        }

        /// <summary>
        /// Renders the typesymbol in a VS style, always showing full name, and any generic type parameters.
        /// </summary>
        Tuple<string, IEnumerable<string>> TypeDisplayStrings(ITypeSymbol t)
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
