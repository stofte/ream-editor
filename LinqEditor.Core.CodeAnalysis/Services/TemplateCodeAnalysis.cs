using LinqEditor.Core.CodeAnalysis.Compiler;
using LinqEditor.Core.CodeAnalysis.Helpers;
using LinqEditor.Core.CodeAnalysis.Models;
using LinqEditor.Core.Context;
using LinqEditor.Core.Helpers;
using LinqEditor.Core.Templates;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LinqEditor.Core.CodeAnalysis.Services
{
    public class TemplateCodeAnalysis : ITemplateCodeAnalysis
    {
        private bool _initialized;
        private MetadataReference[] _references;
        private ITemplateService _templateService;
        private IContext _context;
        private string _initialSource;
        private string _currentSource;
        private int _sourceOffset;
        ExtensionMethodCollection _extensionMethods;

        /// <summary>
        /// Filters by VS semantics (name and kind)
        /// </summary>
        class FilterByNameAndKind : IEqualityComparer<TypeMember>
        {
            public bool Equals(TypeMember x, TypeMember y) { return x.Name == y.Name && x.Kind == y.Kind; }
            public int GetHashCode(TypeMember obj) { return (obj.Name + obj.Kind.ToString()).GetHashCode(); }
        }

        public TemplateCodeAnalysis(ITemplateService templateService, IContext context)
        {
            _initialized = false;
            _templateService = templateService;
            _references = CSharpCompiler.GetStandardReferences();
            _context = context;
            _context.ContextUpdated += Initialize;
        }

        void Initialize(string assemblyPath, string schemaNamespace)
        {
            // update references
            if (!string.IsNullOrEmpty(assemblyPath))
            {
                _references = _references.Concat(new[] { MetadataReference.CreateFromFile(assemblyPath) }).ToArray();
            }

            _currentSource = _initialSource = _templateService.GenerateQuery(Guid.NewGuid(), SchemaConstants.Marker, schemaNamespace);
            var tree = CSharpSyntaxTree.ParseText(_initialSource);
            var semanticModel = GetModel(tree);
            _extensionMethods = CodeAnalysisHelper.GetExtensionMethods(semanticModel);
            _sourceOffset = _initialSource.IndexOf(SchemaConstants.Marker);
            _initialized = true;
        }

        public bool IsReady { get { return _initialized; } }

        public AnalysisResult Analyze(string sourceFragment)
        {
            throw new NotImplementedException();
        }

        public AnalysisResult Analyze(string sourceFragment, int updateIndex)
        {
            var ctx = EditContext.Unknown;
            IEnumerable<SuggestionEntry> suggestions = new List<SuggestionEntry>();

            if (sourceFragment[updateIndex] == '.')
            {
                // check if context is really member access
                _currentSource = _initialSource.Replace(SchemaConstants.Marker, sourceFragment);
                
                var tree = CSharpSyntaxTree.ParseText(_currentSource);
                var semanticModel = GetModel(tree);
                var dotTextSpan = new TextSpan(_sourceOffset + updateIndex, 1);
                var syntaxNode = tree.GetRoot().DescendantNodes(dotTextSpan).Last();
                ctx = syntaxNode is MemberAccessExpressionSyntax ? EditContext.MemberCompletion : EditContext.Unknown;

                if (ctx == EditContext.MemberCompletion)
                {
                    var node = syntaxNode as MemberAccessExpressionSyntax;
                    var typeInfo = semanticModel.GetTypeInfo(node.Expression);
                    var symInfo = semanticModel.GetSymbolInfo(node.Expression);
                    // get all applicable extensions
                    var extensions = CodeAnalysisHelper.GetTypeExtensionMethods(typeInfo, _extensionMethods);
                    var typeInformation = CodeAnalysisHelper.GetTypeInformation(typeInfo, symInfo.Symbol);
                    // possibly use DeclaringSyntaxReferences.Count() == 0
                    var anyPrivate = typeInformation.Members.Where(x => x.Accessibility == AccessibilityModifier.Private).Count();
                    var isStatic = symInfo.Symbol.IsStatic || symInfo.Symbol.Kind == SymbolKind.NamedType;

                    var debugExt = string.Join("|", extensions.Distinct(new TypeMember()).Select(x => x.Name).OrderBy(x => x));
                    var debugMem = string.Join("|", typeInformation.Members.Distinct(new TypeMember()).Select(x => x.Name + ":" + x.Kind).OrderBy(x => x));
                    suggestions = MapSuggestions(typeInformation, extensions, isStatic);
                }
            }

            return new AnalysisResult
            {
                Context = ctx,
                MemberCompletions = suggestions.OrderBy(x => x.Value).ThenBy(x => x.Kind)
            };
        }
        
        private SemanticModel GetModel(SyntaxTree tree)
        {
            var compilerOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
            var comp = CSharpCompilation.Create(Guid.NewGuid().ToIdentifierWithPrefix("d"))
                .AddReferences(_references)
                .WithOptions(compilerOptions)
                .AddSyntaxTrees(tree);

            var diag = comp.GetDiagnostics();
            return comp.GetSemanticModel(tree);
        }

        /// <summary>
        /// Map the type and extension method information for static/non-static 
        /// context into a filtered suggestionentry list for the ui
        /// </summary>
        /// <param name="fullInfo">Full type information.</param>
        /// <param name="extensionMethods">The extension methods.</param>
        /// <param name="staticAccess">if set to <c>true</c>, access is static (if false, access is instance).</param>
        /// <returns></returns>
        private IEnumerable<SuggestionEntry> MapSuggestions(TypeInformation fullInfo, IEnumerable<TypeMember> extensionMethods, bool staticAccess)
        {
            var l = fullInfo.Members
                .Concat(extensionMethods)
                .Where(x => x.IsStatic == staticAccess || x.Kind == MemberKind.ExtensionMethod)
                .Distinct(new FilterByNameAndKind())
                .Select(x => new SuggestionEntry { Kind = x.Kind, Value = x.Name });
            return l;
        }
    }
}
