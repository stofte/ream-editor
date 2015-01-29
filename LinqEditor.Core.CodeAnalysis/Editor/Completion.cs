using LinqEditor.Core.CodeAnalysis.Compiler;
using LinqEditor.Core.CodeAnalysis.Models;
using LinqEditor.Core.Templates;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.CodeAnalysis.Editor
{
    public class Completion : ICompletion
    {
        private ITemplateService _templateService;
        private MetadataReference[] _references;
        private string _initialSource;
        private string _currentSource;
        private int _sourceOffset;
        private string _entryNamespace;
        private string _entryName;
        public static string Marker = "//fragment";
        private IDictionary<string, CompletionEntry> _extensionMethods;

        public Completion(ITemplateService templateService)
        {
            _templateService = templateService;
            _references = CSharpCompiler.GetStandardReferences();
        }

        public void Initialize(string assemblyPath = null, string schemaNamespace = null)
        {
            // update references
            if (!string.IsNullOrEmpty(assemblyPath))
            {
                _references = _references.Concat(new[] { MetadataReference.CreateFromFile(assemblyPath) }).ToArray();
            }

            var queryNamespace = "";
            _currentSource = _initialSource = _templateService.GenerateQuery(Guid.NewGuid(), out queryNamespace, Marker, schemaNamespace);
            var tree = CSharpSyntaxTree.ParseText(_initialSource);
            var semanticModel = GetModel(tree);
            _sourceOffset = _initialSource.IndexOf(Marker);

            // find entry point
            var nodes = tree.GetRoot().DescendantNodes();
            // test code has more then one entry
            var entryMethod = nodes.OfType<MethodDeclarationSyntax>().Last();
            _entryNamespace = nodes.OfType<NamespaceDeclarationSyntax>().Last().Name.ToString();
            _entryName = nodes.OfType<MethodDeclarationSyntax>().Last().Identifier.ValueText;
            var methodBody = entryMethod
                .DescendantNodes().OfType<StatementSyntax>().First();

            // gets static types available at location
            var availableTypes = semanticModel
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

            var dict = new Dictionary<string, CompletionEntry>();
            foreach (var m in availableExtensionMethods)
            {
                var t = m.Parameters.First().Type;
                var key = t.OriginalDefinition != null && t.OriginalDefinition != t ?
                    t.OriginalDefinition.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) :
                    t.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

                if (!dict.ContainsKey(key))
                {
                    dict.Add(key, new CompletionEntry { 
                        Methods = new List<IMethodSymbol>(),
                        Symbol = t
                    });
                }
                dict[key].Methods.Add(m);
            }

            _extensionMethods = dict;
        }

        public void UpdateFragment(string fragment)
        {
            _currentSource = _initialSource.Replace(Marker, fragment);
        }

        public Models.SuggestionList MemberAccessExpressionCompletions(int fragmentIndex)
        {
            var tree = CSharpSyntaxTree.ParseText(_currentSource);
            var semanticModel = GetModel(tree);
            var dotTextSpan = new TextSpan(_sourceOffset + fragmentIndex, 1);
            var memberAccessNode = (MemberAccessExpressionSyntax)tree.GetRoot().DescendantNodes(dotTextSpan).Last();
            
            var lhs = semanticModel.GetTypeInfo(memberAccessNode.Expression);
            var symInfo = semanticModel.GetSymbolInfo(memberAccessNode.Expression);

            var typeEntries = GetTypeEntries(lhs, symInfo.Symbol);
            var typeExtensions = GetTypeExtensionMethods(lhs);

            var list = new SuggestionList
            {
                // vs sorts by regular methods first, then by extensions
                Suggestions = typeExtensions.Concat(typeEntries).OrderBy(x => x.Value).ThenBy(x => x.Kind)
            };

            return list;
        }

        private SemanticModel GetModel(SyntaxTree tree)
        {
            var compilerOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
            var comp = CSharpCompilation.Create("d" + Guid.NewGuid().ToString().Replace("-", ""))
                .AddReferences(_references)
                .WithOptions(compilerOptions)
                .AddSyntaxTrees(tree);
            
            var diag = comp.GetDiagnostics();
            return comp.GetSemanticModel(tree);
        }

        private IEnumerable<SuggestionEntry> GetTypeEntries(TypeInfo typeInfo, ISymbol symbolInfo)
        {
            var t = typeInfo.Type;
            
            var isStatic = symbolInfo.IsStatic || symbolInfo.Kind == SymbolKind.NamedType;
            var isContainer = typeInfo.Type.ContainingNamespace.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat) == _entryNamespace;
            
            // dont include the entry point in completions
            var methods = isContainer ? new SuggestionEntry[] { } :
                t.GetMembers().OfType<IMethodSymbol>()
                .Where(x => x.CanBeReferencedByName && x.IsStatic == isStatic)
                .Select(x => x.Name)
                .Distinct()
                .Select(x => new SuggestionEntry { Kind = MemberKind.Method, Value = x });

            var properties = t.GetMembers().OfType<IPropertySymbol>()
                .Where(x => x.CanBeReferencedByName && !x.IsIndexer &&
                    ((!isContainer && x.IsStatic == isStatic) ||
                    // if querying the container, we want to include protected properties
                    (isContainer && x.DeclaredAccessibility == Accessibility.Protected)))
                .Select(x => x.Name)
                .Distinct()
                .Select(x => new SuggestionEntry { Kind = MemberKind.Property, Value = x });

            var fields = t.GetMembers().OfType<IFieldSymbol>()
                .Where(x => x.CanBeReferencedByName && x.IsStatic && isStatic)
                .Select(x => x.Name)
                .Distinct()
                .Select(x => new SuggestionEntry { Kind = MemberKind.Field, Value = x });

            var objectEntries = (
                // no references to object methods on container
                isContainer ? new SuggestionEntry[] { } :
                isStatic ? new[]
            {
                new SuggestionEntry {Value = "Equals", Kind = MemberKind.Method },
                new SuggestionEntry {Value = "ReferenceEquals", Kind = MemberKind.Method }
            } : new[] { 
                new SuggestionEntry{ Value = "Equals", Kind = MemberKind.Method },
                new SuggestionEntry{ Value = "GetHashCode", Kind = MemberKind.Method },
                new SuggestionEntry{ Value = "GetType", Kind = MemberKind.Method },
                new SuggestionEntry{ Value = "ToString", Kind = MemberKind.Method },
                // filter out entries that appear in methods
            }).Where(x => methods.All(y => y.Value != x.Value));

            return methods.Concat(properties).Concat(fields).Concat(objectEntries);
        }

        private IEnumerable<SuggestionEntry> GetTypeExtensionMethods(TypeInfo typeInfo)
        {
            // fqn of interfaces the type implements
            var interfaceNames = typeInfo.Type.AllInterfaces.Select(x =>
                x.ConstructedFrom != null && x.ConstructedFrom != x ?
                x.ConstructedFrom.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) :
                x.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));

            // unique names for extensions methods matching interfaces
            var possibleExtensions = interfaceNames
                .SelectMany(x => _extensionMethods.ContainsKey(x) ? _extensionMethods[x].Methods.Select(m => m.Name).Distinct() : new string[] { })
                .Distinct()
                .Select(x => new SuggestionEntry { Kind = MemberKind.ExtensionMethod, Value = x });

            return possibleExtensions;
        }
    }
}
