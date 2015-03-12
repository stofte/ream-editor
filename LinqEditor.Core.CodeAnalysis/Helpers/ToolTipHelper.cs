using LinqEditor.Core.CodeAnalysis.Services;
using LinqEditor.Core.Models.Analysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                    var docId = nodeProp.OriginalDefinition != null && !nodeProp.Equals(nodeProp.OriginalDefinition) ?
                        nodeProp.OriginalDefinition.GetDocumentationCommentId() : nodeProp.GetDocumentationCommentId();
                    docElm = _documentationService.GetDocumentation(docId);
                    tooltip.ItemName = PropertyDisplayStrings(nodeProp, docElm);
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
            // when getting the nodes for an index, the syntax tree will be flattened,
            // so we can scan for patterns in the tree, to determine the best node match.
            // by searching the entire node list and listing all matches, the last match
            // should be the desired node.

            // includes: node offset, priority
            var patterns = new List<Tuple<int, int, Type[]>>
            {
                // method reference
                Tuple.Create(0, 1, new Type[]{ typeof(InvocationExpressionSyntax), typeof(MemberAccessExpressionSyntax), typeof(IdentifierNameSyntax) }),
                // property/field reference
                Tuple.Create(0, 0, new Type[]{ typeof(MemberAccessExpressionSyntax), typeof(IdentifierNameSyntax) }),
                Tuple.Create(1, 0, new Type[]{ typeof(MemberAccessExpressionSyntax), typeof(PredefinedTypeSyntax) }),

                // initial type references ala string/int/float, etc
                Tuple.Create(1, 0, new Type[]{ typeof(VariableDeclarationSyntax), typeof(PredefinedTypeSyntax) }),
                // initial type references ala Some.Where.Foo (+var)
                Tuple.Create(1, 0, new Type[]{ typeof(VariableDeclarationSyntax), typeof(IdentifierNameSyntax) }),

                // generic type params
                Tuple.Create(1, 0, new Type[]{ typeof(TypeArgumentListSyntax), typeof(PredefinedTypeSyntax) }),
                Tuple.Create(1, 0, new Type[]{ typeof(TypeArgumentListSyntax), typeof(GenericNameSyntax) }),

                // object creation
                Tuple.Create(0, 0, new Type[]{ typeof(ObjectCreationExpressionSyntax) }),
            };
            
            var tree = _model.SyntaxTree.GetRoot();
            var allNodes = tree.DescendantNodes(new TextSpan(index, 1));
            var nodeIndex = 0;
            var patIndex = 0;
            var matches = new List<SyntaxNode>();
            foreach (var node in allNodes)
            {
                patIndex = 0;
                foreach (var pat in patterns)
                {
                    if (node.GetType() == pat.Item3[0])
                    {
                        // check remaining nodes
                        var matched = pat.Item3.Length == 1;
                        for (var j = 1; j < pat.Item3.Length &&
                            allNodes.ElementAt(nodeIndex + j).GetType() == pat.Item3[j]; j++)
                        {
                            if (j + 1 >= pat.Item3.Length)
                            {
                                matched = true;
                            }
                        }

                        if (matched)
                        {
                            matches.Add(allNodes.ElementAt(nodeIndex + pat.Item1));
                        }
                    }
                    patIndex++;
                }
                nodeIndex++;
            }

            if (matches.Count > 0)
            {
                return matches[matches.Count - 1];
            }

            return null;
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
        /// Renders the methodsymbol in a VS style, showing number of overloads available,
        /// and for regular methods, return types.
        /// </summary>
        string PropertyDisplayStrings(IPropertySymbol p, DocumentationElement docElm)
        {
            var container = GetBasicName(p.ContainingType, includeNamespaces: false, useAliases: true);
            var retStr = GetBasicName(p.Type, includeNamespaces: false, useAliases: true);
            return string.Format("{0} {1}.{2}", retStr, container, p.Name);
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
                        string.Join(", ", namedT.TypeParameters.Select((x, i) =>
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

        string GetBasicName(ITypeSymbol t, bool includeNamespaces = true, bool useAliases = false)
        {
            // some types have aliases (lower cased "string" aliases "String", etc), for now, to get this rendering, 
            // ToDisplayString must be used, instead of manually rendering the type name.
            var aliasedTypes = new string[] 
            { 
                "T:System.Int32",
                "T:System.Object"
            };
            if (useAliases && aliasedTypes.Contains(t.GetDocumentationCommentId()))
            {
                return t.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
            }
            
            var nameParts = new List<string>();
            nameParts.Add(t.Name); // todo: generics?
            if (includeNamespaces)
            {
                var ns = t.ContainingNamespace;
                do
                {
                    if (!string.IsNullOrWhiteSpace(ns.Name))
                    {
                        nameParts.Add(ns.Name);
                    }
                    ns = ns.ContainingNamespace;
                }
                while (ns != null);
            }

            nameParts.Reverse();
            var baseName = string.Join(".", nameParts);

            var namedType = t as INamedTypeSymbol;
            if (namedType != null && namedType.TypeArguments.Count() > 0)
            {
                baseName += string.Format("<{0}>", string.Join(", ", namedType.TypeArguments.Select(x => GetBasicName(x, includeNamespaces, useAliases))));
            }
            return baseName;
        }
    }
}
