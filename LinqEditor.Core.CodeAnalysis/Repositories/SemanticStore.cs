using LinqEditor.Core.CodeAnalysis.Models;
using LinqEditor.Core.Context;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.CodeAnalysis.Repositories
{
    /// <summary>
    /// Abstracts most roslyn internals, and exposes them via custom dtos.
    /// The class has three phases, during initialize the type space is 
    /// searched for extensionmethods. During editor usage, the latest
    /// model is injected, with optional edit index, which updates state.
    /// The class can then be queryed for relevant context infomation.
    /// </summary>
    public class SemanticStore : ISemanticStore
    {
        private IDictionary<string, IList<TypeMember>> _extensionMethods;

        private string _entryNamespace;
        private string _entryName;
        private SemanticModel _model;

        public void Initialize(SemanticModel model)
        {
            _model = model;
            // find entry point
            var nodes = _model.SyntaxTree.GetRoot().DescendantNodes();
            // test code has more then one entry
            var entryMethod = nodes.OfType<MethodDeclarationSyntax>().Last();
            _entryNamespace = nodes.OfType<NamespaceDeclarationSyntax>().Last().Name.ToString();
            _entryName = nodes.OfType<MethodDeclarationSyntax>().Last().Identifier.ValueText;
            var methodBody = entryMethod
                .DescendantNodes().OfType<StatementSyntax>().First();

            // gets static types available at location
            var availableTypes = _model
                .LookupSymbols(methodBody.Span.Start)
                .OfType<INamedTypeSymbol>()
                .Where(x => x.CanBeReferencedByName && x.IsStatic && !x.IsAbstract &&
                    !x.ContainingNamespace.Name.StartsWith("IQToolkit"));

            // lookup extension methods on available types
            var availableExtensionMethods = new List<IMethodSymbol>();
            foreach (var type in availableTypes)
            {
                var methods = type.GetMembers().OfType<IMethodSymbol>();
                var extensions = methods.Where(m => m.IsExtensionMethod && m.CanBeReferencedByName);
                availableExtensionMethods.AddRange(extensions);
            }

            var dict = new Dictionary<string, IList<TypeMember>>();
            foreach (var m in availableExtensionMethods)
            {
                var t = m.Parameters.First().Type;
                var key = t.OriginalDefinition != null && t.OriginalDefinition != t ?
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

            _extensionMethods = dict;
        }

        public void Update(SemanticModel model) 
        {
            _model = model;
        }

        public void UpdateWithEdit(SemanticModel model, int updateIndex) {}

        public TypeInformation GetInformation()
        {
            throw new NotImplementedException();
        }

        private TypeInformation GetTypeEntries(TypeInfo typeInfo, ISymbol symbolInfo)
        {
            var t = typeInfo.Type;

            var isStatic = symbolInfo.IsStatic || symbolInfo.Kind == SymbolKind.NamedType;
            var isContainer = typeInfo.Type.ContainingNamespace.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat) == _entryNamespace;

            Func<Accessibility, AccessibilityModifier> mapAccess = (mod) =>
                mod == Accessibility.Public ? AccessibilityModifier.Public :
                mod == Accessibility.Protected ? AccessibilityModifier.Protected :
                AccessibilityModifier.Private;

            var extensions = GetTypeExtensionMethods(t);

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

            var objectEntries = new [] {
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


        private IEnumerable<TypeMember> GetTypeExtensionMethods(ITypeSymbol symbol)
        {
            // fqn of interfaces the type implements
            var interfaceNames = symbol.AllInterfaces.Select(x =>
                x.ConstructedFrom != null && x.ConstructedFrom != x ?
                x.ConstructedFrom.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) :
                x.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));

            // unique names for extensions methods matching interfaces
            var possibleExtensions = interfaceNames
                .SelectMany(x => _extensionMethods.ContainsKey(x) ? _extensionMethods[x].Select(m => m.Name).Distinct() : new string[] { })
                .Distinct()
                .Select(x => new TypeMember { Kind = MemberKind.ExtensionMethod, IsStatic = true, Accessibility = AccessibilityModifier.Public, Name = x });

            return possibleExtensions;
        }
    }
}
