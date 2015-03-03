using LinqEditor.Core.CodeAnalysis.Services;
using LinqEditor.Core.Models.Analysis;
using LinqEditor.Core.Models.Database;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace LinqEditor.Core.CodeAnalysis.Helpers
{
    public static class CodeAnalysisHelper
    {
        /// <summary>
        /// Extension methods that apply to all types (T), will be entered in the dictionary 
        /// using this key to avoid conflicting with other legal identifiers.
        /// </summary>
        public const string UniversalTypeKey = "*";

        /// <summary>
        /// Attempts to find the most relevant tooltip data for the passed nodes.
        /// </summary>
        public static ToolTipData GetToolTip(IEnumerable<SyntaxNode> nodes, SemanticModel model,
            IDocumentationService documentationService, IEnumerable<INamedTypeSymbol> availableSymbols)
        {
            ToolTipData tooltip = new ToolTipData();

            var varNode = nodes.OfType<VariableDeclarationSyntax>().LastOrDefault();
            var preNode = nodes.OfType<PredefinedTypeSyntax>().FirstOrDefault();
            var idNode = nodes.OfType<IdentifierNameSyntax>().FirstOrDefault();
            var ctorNode = nodes.OfType<ObjectCreationExpressionSyntax>().FirstOrDefault();

            var isInitialNode = varNode != null && preNode != null && preNode.Span.Start == varNode.SpanStart ||
                idNode != null && idNode.SpanStart == varNode.SpanStart;

            var isCtorExpr = ctorNode != null;

            if (isInitialNode)
            {
                var typeInfo = model.GetTypeInfo(varNode.Type);
                var symInfo = model.GetSymbolInfo(varNode.Type);
                var t = typeInfo.Type;
                var docMemberId = t.OriginalDefinition != null && t != t.OriginalDefinition ?
                    t.OriginalDefinition.GetDocumentationCommentId() : t.GetDocumentationCommentId();

                var docs = documentationService.GetDocumentation(docMemberId);
                var nameAndTypes = CodeAnalysisHelper.GetDisplayNameAndSpecializations(typeInfo, symInfo);

                tooltip.TypeAndName = nameAndTypes.Item1;
                tooltip.Specializations = nameAndTypes.Item2;
                tooltip.Description = docs != null ? docs.Summary : string.Empty;
            }
            else if (isCtorExpr)
            {
                var symInfo = model.GetSymbolInfo(ctorNode.Type);
                // obtains symbols of any passed arguments
                var argSymbols = ctorNode.ArgumentList.Arguments.Select(x => model.GetTypeInfo(x.Expression));
                // match the ctor based on the arguments
                var calledCtor = (symInfo.Symbol as INamedTypeSymbol)
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
                    var parts = t.ToDisplayParts();
                    var docMemberId = t.GetDocumentationCommentId();
                    var doccsss = t.GetDocumentationCommentXml(expandIncludes: true);
                    var docs = documentationService.GetDocumentation(docMemberId);
                }
            }

            return tooltip;
        }

        /// <summary>
        /// Returns the VS style display name of the passed type, along with any type specializations.
        /// </summary>
        public static Tuple<string, IEnumerable<string>> GetDisplayNameAndSpecializations(TypeInfo type, SymbolInfo symbol)
        {
            var t = type.Type;
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
                    var length = name.IndexOf(">") - firstIdx;
                    name = name.Replace(name.Substring(firstIdx, length + 1), varianceStr);
                }
            }

            return Tuple.Create(string.Format("{0} {1}", kind, name), specializations.AsEnumerable());
        }

        public static string GetBasicName(ITypeSymbol t)
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
            return string.Join(".", nss);
        }

        public static IEnumerable<Warning> GetWarnings(ImmutableArray<Diagnostic> diagnostics)
        {
            return diagnostics.Where(w => w.Severity == DiagnosticSeverity.Warning).Select(x =>
            {
                var loc = x.Location.GetMappedLineSpan().Span;
                return new Warning
                {
                    Location = new LocationSpan
                    {
                        StartLine = loc.Start.Line + 1,
                        StartColumn = loc.Start.Character + 1,
                        EndLine = loc.End.Line + 1,
                        EndColumn = loc.End.Character + 1
                    },
                    Message = x.GetMessage()
                };
            });
        }

        public static IEnumerable<Error> GetErrors(ImmutableArray<Diagnostic> diagnostics, int markerOffset = 0)
        {
            return diagnostics.Where(e => e.Severity == DiagnosticSeverity.Error).Select(x =>
            {
                var loc = x.Location.GetMappedLineSpan().Span;
                return new Error
                {
                    // errors are for display purposes, so offsetting one
                    Location = new LocationSpan
                    {
                        // the index is for the editor
                        StartIndex = x.Location.SourceSpan.Start,
                        StartLine = loc.Start.Line + 1,
                        StartColumn = loc.Start.Character + 1,
                        EndIndex = x.Location.SourceSpan.End,
                        EndLine = loc.End.Line + 1,
                        EndColumn = loc.End.Character + 1
                    },
                    Message = x.GetMessage()
                };
            });
        }

        public static IEnumerable<TypeMember> GetTypeExtensionMethods(TypeInfo typeInfo, ExtensionMethodCollection extensionMethods)
        {
            // fqn of interfaces the type implements
            var interfaceNames = typeInfo.Type.AllInterfaces.Select(x =>
                x.ConstructedFrom != null && x.ConstructedFrom != x ?
                x.ConstructedFrom.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) :
                x.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));

            // extensionmethods matching interfaces
            var possibleExtensions = interfaceNames
                .SelectMany(x => extensionMethods.ContainsKey(x) ? extensionMethods[x] : new List<TypeMember>());

            // add on any universal type entries
            return possibleExtensions.Concat(extensionMethods[UniversalTypeKey]);
        }


        // todo: anonymous?
        public static TypeInformation GetTypeInformation(INamedTypeSymbol t, string entryClass, string schemaNs = null, DatabaseSchema schema = null)
        {
            var objectEntries = new[] {
                new TypeMember { IsStatic = true, Accessibility = AccessibilityModifier.Public, Kind = MemberKind.Method, Name = "Equals" },
                new TypeMember { IsStatic = true, Accessibility = AccessibilityModifier.Public, Kind = MemberKind.Method, Name = "ReferenceEquals" },

                new TypeMember { IsStatic = false, Accessibility = AccessibilityModifier.Public, Kind = MemberKind.Method, Name = "Equals" },
                new TypeMember { IsStatic = false, Accessibility = AccessibilityModifier.Public, Kind = MemberKind.Method, Name = "GetHashCode" },
                new TypeMember { IsStatic = false, Accessibility = AccessibilityModifier.Public, Kind = MemberKind.Method, Name = "GetType" },
                new TypeMember { IsStatic = false, Accessibility = AccessibilityModifier.Public, Kind = MemberKind.Method, Name = "ToString" }
            };

            //var t = typeInfo.Type as INamedTypeSymbol;

            Func<Accessibility, AccessibilityModifier> mapAccess = (mod) =>
                mod == Accessibility.Public ? AccessibilityModifier.Public :
                mod == Accessibility.Protected ? AccessibilityModifier.Protected :
                AccessibilityModifier.Private;
            
            var methods = t.GetMembers().OfType<IMethodSymbol>()
                .Where(x => x.CanBeReferencedByName)
                .Select(x => new TypeMember
                {
                    IsStatic = x.IsStatic,
                    Accessibility = mapAccess(x.DeclaredAccessibility),
                    Kind = MemberKind.Method,
                    Name = x.Name
                });

            Func<IPropertySymbol, MemberKind> mapPropery = (prop) =>
            {
                var typeStr = GetBasicName(prop.Type) ?? string.Empty;
                var containerStr = GetBasicName(prop.ContainingType) ?? string.Empty;

                return typeStr.StartsWith("IQToolkit.IEntityTable") ? MemberKind.DatabaseTable :
                    !string.IsNullOrWhiteSpace(schemaNs) && containerStr.StartsWith(schemaNs) ? MemberKind.TableColumn : MemberKind.Property;
            };

            var properties = t.GetMembers().OfType<IPropertySymbol>()
                .Where(x => x.CanBeReferencedByName && !x.IsIndexer)
                .Select(x => new TypeMember
                {
                    IsStatic = x.IsStatic,
                    Accessibility = mapAccess(x.DeclaredAccessibility),
                    Kind = mapPropery(x),
                    Name = x.Name
                });

            var fields = t.GetMembers().OfType<IFieldSymbol>()
                .Where(x => x.CanBeReferencedByName)
                .Select(x => new TypeMember
                {
                    IsStatic = x.IsStatic,
                    Accessibility = mapAccess(x.DeclaredAccessibility),
                    Kind = MemberKind.Field,
                    Name = x.Name
                });

            return new TypeInformation
            {
                IsStatic = t.IsStatic,
                Name = t.Name,
                Namespace = t.ContainingNamespace.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                Members = methods.Concat(properties).Concat(fields).Concat(objectEntries),
                EntryClass = GetBasicName(t) == entryClass
            };
        }

        /// <summary>
        /// Gets the extension methods available inside the scope of the last method.
        /// </summary>
        public static ExtensionMethodCollection GetExtensionMethods(SemanticModel model)
        {
            // find entry point
            var nodes = model.SyntaxTree.GetRoot().DescendantNodes();
            // test code has more then one entry
            var entryMethod = nodes.OfType<MethodDeclarationSyntax>().Last();
            var methodBody = entryMethod
                .DescendantNodes().OfType<StatementSyntax>().First();

            // gets static types available at location
            var availableTypes = model
                .LookupSymbols(methodBody.Span.Start)
                .OfType<INamedTypeSymbol>()
                .Where(x => x.CanBeReferencedByName && x.IsStatic && !x.IsAbstract);

            var foo = availableTypes.Where(x => x.Name == "Dumper");

            var foox = foo.FirstOrDefault();

            // lookup extension methods on available types
            var availableExtensionMethods = new List<IMethodSymbol>();
            foreach (var type in availableTypes)
            {
                var methods = type.GetMembers().OfType<IMethodSymbol>();
                var extensions = methods.Where(m => m.IsExtensionMethod && m.CanBeReferencedByName);
                availableExtensionMethods.AddRange(extensions);
            }

            var dict = new ExtensionMethodCollection();
            // always add this list
            dict.Add(UniversalTypeKey, new List<TypeMember>());
            foreach (var m in availableExtensionMethods)
            {
                var t = m.Parameters.First().Type;

                var key = t.TypeKind == TypeKind.TypeParameter ? UniversalTypeKey :
                    t.OriginalDefinition != null && t.OriginalDefinition != t ?
                    t.OriginalDefinition.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) :
                    t.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

                if (!dict.ContainsKey(key))
                {
                    dict.Add(key, new List<TypeMember>());
                }
                dict[key].Add(new TypeMember
                {
                    Accessibility = AccessibilityModifier.Public,
                    IsStatic = true,
                    Kind = MemberKind.ExtensionMethod,
                    Name = m.Name
                });
            }

            return dict;
        }
    }
}
