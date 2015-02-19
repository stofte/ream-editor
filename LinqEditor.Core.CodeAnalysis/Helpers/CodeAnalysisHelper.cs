using LinqEditor.Core.Models.Analysis;
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

        public static IEnumerable<Error> GetErrors(ImmutableArray<Diagnostic> diagnostics)
        {
            return diagnostics.Where(e => e.Severity == DiagnosticSeverity.Error).Select(x =>
            {
                var loc = x.Location.GetMappedLineSpan().Span;
                return new Error
                {
                    // errors are for display purposes, so offsetting one
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

        public static TypeInformation GetTypeInformation(TypeInfo typeInfo, ISymbol symbolInfo)
        {
            var t = typeInfo.Type;

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

            var properties = t.GetMembers().OfType<IPropertySymbol>()
                .Where(x => x.CanBeReferencedByName && !x.IsIndexer)
                .Select(x => new TypeMember
                {
                    IsStatic = x.IsStatic,
                    Accessibility = mapAccess(x.DeclaredAccessibility),
                    Kind = MemberKind.Property,
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

            var objectEntries = new[] {
                new TypeMember { IsStatic = true, Accessibility = AccessibilityModifier.Public, Kind = MemberKind.Method, Name = "Equals" },
                new TypeMember { IsStatic = true, Accessibility = AccessibilityModifier.Public, Kind = MemberKind.Method, Name = "ReferenceEquals" },

                new TypeMember { IsStatic = false, Accessibility = AccessibilityModifier.Public, Kind = MemberKind.Method, Name = "Equals" },
                new TypeMember { IsStatic = false, Accessibility = AccessibilityModifier.Public, Kind = MemberKind.Method, Name = "GetHashCode" },
                new TypeMember { IsStatic = false, Accessibility = AccessibilityModifier.Public, Kind = MemberKind.Method, Name = "GetType" },
                new TypeMember { IsStatic = false, Accessibility = AccessibilityModifier.Public, Kind = MemberKind.Method, Name = "ToString" }
            };

            return new TypeInformation
            {
                IsStatic = t.IsStatic,
                Name = t.Name,
                Namespace = t.ContainingNamespace.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                Members = methods.Concat(properties).Concat(fields).Concat(objectEntries)
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
