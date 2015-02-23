using LinqEditor.Core.CodeAnalysis.Compiler;
using LinqEditor.Core.CodeAnalysis.Helpers;
using LinqEditor.Core.Helpers;
using LinqEditor.Core.Models.Analysis;
using LinqEditor.Core.Templates;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace LinqEditor.Core.CodeAnalysis.Services
{
    public class TemplateCodeAnalysis : ITemplateCodeAnalysis
    {
        private bool _initialized;
        private MetadataReference[] _references;
        private ITemplateService _templateService;
        private string _initialSource;
        private string _currentSource;
        private int _sourceOffset;
        ExtensionMethodCollection _extensionMethods;
        IDocumentationService _documentationService;
        
        /// <summary>
        /// Filters by VS semantics (name and kind)
        /// </summary>
        class FilterByNameAndKind : IEqualityComparer<TypeMember>
        {
            public bool Equals(TypeMember x, TypeMember y) { return x.Name == y.Name && x.Kind == y.Kind; }
            public int GetHashCode(TypeMember obj) { return (obj.Name + obj.Kind.ToString()).GetHashCode(); }
        }

        public TemplateCodeAnalysis(ITemplateService templateService, IDocumentationService documentationService)
        {
            DebugLogger.Log(GetHashCode());
            _initialized = false;
            _templateService = templateService;
            _documentationService = documentationService;
            _references = CSharpCompiler.GetStandardReferences();
        }

        public void Initialize(string assemblyPath = null)
        {
            // todo: dont depend on filename ...
            Initialize(assemblyPath, assemblyPath != null ? Path.GetFileNameWithoutExtension(assemblyPath) : string.Empty);
        }

        private void Initialize(string assemblyPath, string schemaNamespace)
        {
            // update references
            if (!string.IsNullOrEmpty(assemblyPath))
            {
                _references = _references.Concat(new[] { MetadataReference.CreateFromFile(assemblyPath) }).ToArray();
                _currentSource = _initialSource = _templateService.GenerateQuery(Guid.NewGuid(), SchemaConstants.Marker, schemaNamespace);
            }
            else
            {
                _currentSource = _initialSource = _templateService.GenerateCodeStatements(Guid.NewGuid(), SchemaConstants.Marker);
            }
            
            var tree = CSharpSyntaxTree.ParseText(_initialSource);
            IEnumerable<Warning> warnings = new List<Warning>();
            IEnumerable<Error> errors = new List<Error>();
            var semanticModel = GetModelAndDiagnostics(tree, out warnings, out errors);
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
            DebugLogger.Log("src(" + updateIndex + ")=" + sourceFragment);
            var ctx = UserContext.Unknown;
            IEnumerable<CompletionEntry> suggestions = new List<CompletionEntry>();
            IEnumerable<Warning> warnings = new List<Warning>();
            IEnumerable<Error> errors = new List<Error>();
            ToolTipData tooltip = new ToolTipData();
            
            _currentSource = _initialSource.Replace(SchemaConstants.Marker, sourceFragment);
            var tree = CSharpSyntaxTree.ParseText(_currentSource);
            var semanticModel = GetModelAndDiagnostics(tree, out warnings, out errors);


            var oneCharTextSpan = new TextSpan(_sourceOffset + updateIndex, 1);
            

            if (sourceFragment[updateIndex] == '.')
            {
                // check if context is really member access
                var syntaxNode = tree.GetRoot()
                    .DescendantNodes(oneCharTextSpan)
                    .OfType<MemberAccessExpressionSyntax>()
                    .LastOrDefault();
                
                if (syntaxNode != null)
                {
                    if (syntaxNode.Expression == null) goto exit;
                    var typeInfo = semanticModel.GetTypeInfo(syntaxNode.Expression);
                    var symInfo = semanticModel.GetSymbolInfo(syntaxNode.Expression);
                    if (symInfo.Symbol == null) goto exit;
                    // should be ok to work with symbols
                    ctx = UserContext.MemberCompletion;
                    // get all applicable extensions
                    var extensions = CodeAnalysisHelper.GetTypeExtensionMethods(typeInfo, _extensionMethods);
                    var typeInformation = CodeAnalysisHelper.GetTypeInformation(typeInfo, symInfo.Symbol);
                    // possibly use DeclaringSyntaxReferences.Count() == 0
                    var anyPrivate = typeInformation.Members.Where(x => x.Accessibility == AccessibilityModifier.Private).Count();
                    var isStatic = symInfo.Symbol.IsStatic || symInfo.Symbol.Kind == SymbolKind.NamedType;

                    suggestions = MapSuggestions(typeInformation, extensions, isStatic);
                }
            }
            else
            {
                var nodes = tree.GetRoot().DescendantNodes(oneCharTextSpan);
                // for tooltips, we need a var decl, to obtain the actual type to show
                var varNode = nodes
                    .OfType<VariableDeclarationSyntax>()
                    .LastOrDefault();

                var allNodes = nodes
                    .OfType<CSharpSyntaxNode>();

                var preNode = nodes
                    .OfType<PredefinedTypeSyntax>()
                    .FirstOrDefault();

                var idNode = nodes
                    .OfType<IdentifierNameSyntax>()
                    .FirstOrDefault();

                if (varNode != null)
                {
                    var isInitialNode = preNode != null && preNode.Span.Start == varNode.SpanStart ||
                        idNode != null && idNode.SpanStart == varNode.SpanStart;

                    if (!isInitialNode) goto exit;

                    var typeInfo = semanticModel.GetTypeInfo(varNode.Type);
                    var symInfo = semanticModel.GetSymbolInfo(varNode.Type);
                    var t = typeInfo.Type;
                    var docMemberId = t.OriginalDefinition != null && t != t.OriginalDefinition ?
                        t.OriginalDefinition.GetDocumentationCommentId() : t.GetDocumentationCommentId();

                    var docs = _documentationService.GetDocumentation(docMemberId);

                    var nameAndTypes = CodeAnalysisHelper.GetDisplayNameAndSpecializations(typeInfo, symInfo);

                    tooltip.TypeAndName = nameAndTypes.Item1;
                    tooltip.Description = docs != null ? docs.Element("summary").Value : string.Empty;

                    if (!string.IsNullOrWhiteSpace(tooltip.TypeAndName) &&
                        !string.IsNullOrWhiteSpace(tooltip.Description))
                    {
                        ctx = UserContext.ToolTip;
                    }
                }
            }

            exit:
            return new AnalysisResult
            {
                Context = ctx,
                MemberCompletions = suggestions.OrderBy(x => x.Value).ThenBy(x => x.Kind),
                Errors = errors,
                Warnings = warnings,
                ToolTip = tooltip
            };
        }

        private SemanticModel GetModelAndDiagnostics(SyntaxTree tree, out IEnumerable<Warning> warnings, out IEnumerable<Error> errors)
        {
            var compilerOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
            var comp = CSharpCompilation.Create(Guid.NewGuid().ToIdentifierWithPrefix(SchemaConstants.QueryPrefix))
                .AddReferences(_references)
                .WithOptions(compilerOptions)
                .AddSyntaxTrees(tree);

            var diag = comp.GetDiagnostics();
            warnings = CodeAnalysisHelper.GetWarnings(diag);
            errors = CodeAnalysisHelper.GetErrors(diag);
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
        private IEnumerable<CompletionEntry> MapSuggestions(TypeInformation fullInfo, IEnumerable<TypeMember> extensionMethods, bool staticAccess)
        {
            var l = fullInfo.Members
                .Concat(extensionMethods)
                .Where(x => x.IsStatic == staticAccess || x.Kind == MemberKind.ExtensionMethod)
                .Distinct(new FilterByNameAndKind())
                .Select(x => new CompletionEntry { Kind = x.Kind, Value = x.Name });
            return l;
        }
    }
}
