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
        public static string Marker = "//fragment";
        private IDictionary<string, CompletionEntry> _extensionMethods;

        public Completion(ITemplateService templateService)
        {
            _templateService = templateService;
            _references = CSharpCompiler.GetStandardReferences();
        }

        public void Initialize(string assemblyPath = null)
        {
            var queryNamespace = "";
            _currentSource = _initialSource = _templateService.GenerateQuery(Guid.NewGuid(), out queryNamespace, "", "Schema");
            var tree = CSharpSyntaxTree.ParseText(_initialSource);
            var semanticModel = GetModel(tree);
            _sourceOffset = _initialSource.IndexOf(Marker);
            // find entry point
            var methodBody = tree.GetRoot()
                .DescendantNodes().OfType<MethodDeclarationSyntax>().Last()
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
            var lhsType = semanticModel.GetTypeInfo(memberAccessNode.Expression).Type;
            var symInfo = semanticModel.GetSymbolInfo(memberAccessNode.Expression);

            var typeEntries = GetTypeEntries(lhs, symInfo.Symbol);
            var typeExtensions = GetTypeExtensionMethods(lhs);

            // methods might contain overrides of int functions i guess
            var list = new SuggestionList
            {
                // vs sorts by regular methods first, then by extensions, 
                // which the enum should ensure
                Suggestions = typeExtensions.Concat(typeEntries).OrderBy(x => x.Value).ThenBy(x => x.Kind)
            };

            return list;
        }

        private SemanticModel GetModel(SyntaxTree tree)
        {
            var compilerOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
            return CSharpCompilation.Create("d" + Guid.NewGuid().ToString().Replace("-", ""))
                .AddReferences(_references)
                .WithOptions(compilerOptions)
                .AddSyntaxTrees(tree)
                .GetSemanticModel(tree);
        }

        private IEnumerable<SuggestionEntry> GetTypeEntries(TypeInfo typeInfo, ISymbol symbolInfo)
        {
            var t = typeInfo.Type;
            var staticAccess = symbolInfo.IsStatic;

            var methods = t.GetMembers().OfType<IMethodSymbol>()
                .Where(x => x.CanBeReferencedByName && x.IsStatic == staticAccess)
                .Select(x => x.Name)
                .Distinct()
                .Select(x => new SuggestionEntry { Kind = SuggestionType.Method, Value = x });

            var properties = t.GetMembers().OfType<IPropertySymbol>()
                .Where(x => x.CanBeReferencedByName && !x.IsIndexer && x.IsStatic == staticAccess)
                .Select(x => x.Name)
                .Distinct()
                .Select(x => new SuggestionEntry { Kind = SuggestionType.Property, Value = x });

            var fields = t.GetMembers().OfType<IFieldSymbol>()
                .Where(x => x.CanBeReferencedByName && x.IsStatic && staticAccess)
                .Select(x => x.Name)
                .Distinct()
                .Select(x => new SuggestionEntry { Kind = SuggestionType.Field, Value = x });

            var objectEntries = (staticAccess ? new[]
            {
                new SuggestionEntry {Value = "Equals", Kind = SuggestionType.Method },
                new SuggestionEntry {Value = "ReferenceEquals", Kind = SuggestionType.Method }
            } : new[] { 
                new SuggestionEntry{ Value = "Equals", Kind = SuggestionType.Method },
                new SuggestionEntry{ Value = "GetHashCode", Kind = SuggestionType.Method },
                new SuggestionEntry{ Value = "GetType", Kind = SuggestionType.Method },
                new SuggestionEntry{ Value = "ToString", Kind = SuggestionType.Method },
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
                .Select(x => new SuggestionEntry { Kind = SuggestionType.ExtensionMethod, Value = x });

            return possibleExtensions;
        }
    }
}
