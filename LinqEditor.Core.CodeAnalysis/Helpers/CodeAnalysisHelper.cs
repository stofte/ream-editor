using LinqEditor.Core.CodeAnalysis.Services;
using LinqEditor.Core.Models.Analysis;
using LinqEditor.Core.Models.Database;
using Microsoft.CodeAnalysis;
using LinqEditor.Core.Helpers;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using LinqEditor.Core.CodeAnalysis.Compiler;
using System.Text.RegularExpressions;

namespace LinqEditor.Core.CodeAnalysis.Helpers
{
    public static class CodeAnalysisHelper
    {
        /// <summary>
        /// Extension methods that apply to all types (T), will be entered in the dictionary 
        /// using this key to avoid conflicting with other legal identifiers.
        /// </summary>
        public const string UniversalTypeKey = "*";

        // todo: this will fail for generic types, etc. need to figure out
        // how to incorporate into tooltiphelper or something
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
                    Message = x.GetMessage(),
                    Code = x.Id
                };
            });
        }

        public static IEnumerable<TypeMember> GetTypeExtensionMethods(ITypeSymbol typeSymbol, ExtensionMethodCollection extensionMethods)
        {
            // fqn of interfaces the type implements
            var interfaceNames = typeSymbol.AllInterfaces.Select(x =>
                x.ConstructedFrom != null && x.ConstructedFrom != x ?
                x.ConstructedFrom.GetDocumentationCommentId() :
                x.GetDocumentationCommentId())
                .Concat(new string[] { 
                    typeSymbol.OriginalDefinition.GetDocumentationCommentId(),
                    typeSymbol.GetDocumentationCommentId()
                }.Where(x => x != null));

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
                string key = null;
                if (t.TypeKind == TypeKind.TypeParameter)
                {
                    key = UniversalTypeKey;
                }
                else if (t is INamedTypeSymbol)
                {
                    var namedT = t as INamedTypeSymbol;
                    if (namedT.TypeArguments.Count() > 0 && namedT.TypeArguments.First().TypeKind != TypeKind.TypeParameter)
                    {
                        key = t.GetDocumentationCommentId();
                    }
                    else
                    {
                        key = t.OriginalDefinition.GetDocumentationCommentId();
                    }
                }
                else
                {
                    throw new Exception("unknown node");
                }

                if (!dict.ContainsKey(key))
                {
                    dict.Add(key, new List<TypeMember>());
                }
                dict[key].Add(new TypeMember
                {
                    Accessibility = AccessibilityModifier.Public,
                    IsStatic = true,
                    Kind = MemberKind.ExtensionMethod,
                    Name = m.Name,
                    DocumentationId = m.GetDocumentationCommentId(),
                    Symbol = m
                });
            }

            return dict;
        }

        public static string CanCompleteTree(string sourceProgram, IEnumerable<MetadataReference> references)
        {
            string sourceFragment = null;
            var tree = CSharpSyntaxTree.ParseText(sourceProgram);
            var replacements = new Dictionary<SyntaxNode, SyntaxNode>();
            var nodes = tree.GetRoot().DescendantNodes().OfType<StatementSyntax>().Where(x => !(x is BlockSyntax));
            foreach (var node in nodes)
            {
                var nodeStr = node.ToString();
                var suffix = nodeStr.Substring(nodeStr.TrimEnd().Length);
                string newStatement = null;
                var localNode = node as LocalDeclarationStatementSyntax;
                var exprNode = node as ExpressionStatementSyntax;
                if (localNode != null && localNode.SemicolonToken.IsMissing)
                {
                    newStatement = string.Format("{0};{1}", nodeStr.TrimEnd(), suffix);
                }
                else if (exprNode != null && exprNode.SemicolonToken.IsMissing)
                {
                    var newNodeStr = nodeStr.TrimEnd();
                    var dumpRegex = new Regex(@"\.\W*Dump\W*(\W*)\W*$", RegexOptions.Multiline);
                    var m = dumpRegex.Match(newNodeStr);
                    if (!m.Success)
                    {
                        newNodeStr += ".Dump()";
                    }
                    newStatement = string.Format("{0};{1}", newNodeStr, suffix);
                }

                if (!string.IsNullOrWhiteSpace(newStatement))
                {
                    var newNode = SyntaxFactory.ParseStatement(newStatement);
                    replacements.Add(node, newNode);
                }
            }

            // check if new tree is error free
            var newRoot = tree.GetRoot().ReplaceNodes(replacements.Keys, (n1, n2) => replacements[n1]);
            var compilation = CSharpCompilation.Create(Guid.NewGuid().ToIdentifierWithPrefix("test"))
                .WithOptions(new CSharpCompilationOptions(outputKind: OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Debug))
                .AddReferences(references)
                .AddSyntaxTrees(new SyntaxTree[] { newRoot.SyntaxTree });

            var model = compilation.GetSemanticModel(newRoot.SyntaxTree);
            var noErrors = GetErrors(model.GetDiagnostics()).Count() == 0;

            if (noErrors)
            {
                sourceFragment = newRoot.ToString();
            }
            else
            {
                sourceFragment = string.Empty;
            }

            return sourceFragment;
        }
    }
}
