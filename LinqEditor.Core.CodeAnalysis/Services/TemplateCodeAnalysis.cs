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
        protected IDocumentationService _documentationService;
        protected ITemplateService _templateService;
        protected MetadataReference[] _references;
        protected ExtensionMethodCollection _extensionMethods; 
        
        // instance data
        protected bool _initialized;
        protected string _initialSource;
        protected int _sourceOffset;
        protected string _namespace;
        protected string _entryClass;

        // current analysis model
        protected string _currentStub;
        protected IEnumerable<Warning> _warnings;
        protected IEnumerable<Error> _errors;
        protected IEnumerable<SyntaxNode> _nodes;
        protected SemanticModel _currentModel;

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

        protected void Initialize(string assemblyPath, string schemaNamespace)
        {
            // update references
            if (!string.IsNullOrEmpty(assemblyPath))
            {
                _references = _references.Concat(new[] { MetadataReference.CreateFromFile(assemblyPath) }).ToArray();
                _initialSource = _templateService.GenerateQuery(Guid.NewGuid(), SchemaConstants.Marker, schemaNamespace);
            }
            else
            {
                _initialSource = _templateService.GenerateCodeStatements(Guid.NewGuid(), SchemaConstants.Marker);
            }
            
            var tree = CSharpSyntaxTree.ParseText(_initialSource);
            IEnumerable<Warning> warnings = new List<Warning>();
            IEnumerable<Error> errors = new List<Error>();
            var semanticModel = GetModelAndDiagnostics(tree, out warnings, out errors);
            _extensionMethods = CodeAnalysisHelper.GetExtensionMethods(semanticModel);
            _sourceOffset = _initialSource.IndexOf(SchemaConstants.Marker);
            _initialized = true;

            var nodes = tree.GetRoot().DescendantNodes();
            var nsNode = nodes.OfType<NamespaceDeclarationSyntax>().LastOrDefault();
            var clsNode = nodes.OfType<ClassDeclarationSyntax>().LastOrDefault();

            _namespace = nsNode.Name.ToString();
            _entryClass = clsNode.Identifier.ToString();
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
            
            var currentSource = _initialSource.Replace(SchemaConstants.Marker, sourceFragment);
            var tree = CSharpSyntaxTree.ParseText(currentSource);
            var semanticModel = GetModelAndDiagnostics(tree, out warnings, out errors);

            var oneCharTextSpan = new TextSpan(_sourceOffset + updateIndex, 1);
            var nodes = tree.GetRoot().DescendantNodes(oneCharTextSpan);
            var allNodes = nodes.OfType<CSharpSyntaxNode>();
            
            if (sourceFragment[updateIndex] == '.')
            {
                // check if context is really member access
                var syntaxNode = nodes.OfType<MemberAccessExpressionSyntax>().LastOrDefault();
                
                if (syntaxNode != null && syntaxNode.Expression != null)
                {
                    var clsNode = tree.GetRoot().DescendantNodes()
                        .OfType<ClassDeclarationSyntax>().LastOrDefault();
                    var nsNode = tree.GetRoot().DescendantNodes()
                        .OfType<NamespaceDeclarationSyntax>().LastOrDefault();

                    // used for filtering the this object specially
                    var entryClass = string.Format("{0}.{1}", nsNode.Name, clsNode.Identifier);

                    var typeInfo = semanticModel.GetTypeInfo(syntaxNode.Expression);
                    var symInfo = semanticModel.GetSymbolInfo(syntaxNode.Expression);

                    if (symInfo.Symbol == null) goto exit;
                    // should be ok to work with symbols
                    ctx = UserContext.MemberCompletion;
                    // get all applicable extensions
                    var extensions = CodeAnalysisHelper.GetTypeExtensionMethods(typeInfo, _extensionMethods);
                    TypeInformation mainType = CodeAnalysisHelper.GetTypeInformation(typeInfo.Type as INamedTypeSymbol, entryClass);
                    TypeInformation subType = null;
                    var isStatic = symInfo.Symbol.IsStatic || symInfo.Symbol.Kind == SymbolKind.NamedType;

                    if (mainType.EntryClass)
                    {
                        subType = CodeAnalysisHelper.GetTypeInformation(typeInfo.Type.BaseType, entryClass);

                        var modifiedType = new TypeInformation
                        {
                            EntryClass = true,
                            Name = mainType.Name,
                            Namespace = mainType.Namespace,
                            Members = subType.Members.Where(x => x.Kind == MemberKind.Property)
                        };

                        suggestions = MapSuggestions(modifiedType, new List<TypeMember>(), isStatic);
                    }
                    else
                    {
                        
                        suggestions = MapSuggestions(mainType, extensions, isStatic);
                    }
                    // possibly use DeclaringSyntaxReferences.Count() == 0
                    //var anyPrivate = typeInformation.Members.Where(x => x.Accessibility == AccessibilityModifier.Private).Count();
                }
            }
            else
            {
                // for tooltips, we need a var decl, to obtain the actual type to show
                var varNode = nodes.OfType<VariableDeclarationSyntax>().LastOrDefault();
                var preNode = nodes.OfType<PredefinedTypeSyntax>().FirstOrDefault();
                var idNode = nodes.OfType<IdentifierNameSyntax>().FirstOrDefault();

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
                    tooltip.Specializations = nameAndTypes.Item2;
                    tooltip.Description = docs != null ? docs.Element("summary").Value : string.Empty;

                    if (!string.IsNullOrWhiteSpace(tooltip.TypeAndName))
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

        protected SemanticModel GetModelAndDiagnostics(SyntaxTree tree, out IEnumerable<Warning> warnings, out IEnumerable<Error> errors)
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
        /// Filters by VS semantics (name and kind)
        /// </summary>
        protected class FilterByNameAndKind : IEqualityComparer<TypeMember>
        {
            public bool Equals(TypeMember x, TypeMember y) { return x.Name == y.Name && x.Kind == y.Kind; }
            public int GetHashCode(TypeMember obj) { return (obj.Name + obj.Kind.ToString()).GetHashCode(); }
        }

        /// <summary>
        /// Map the type and extension method information for static/non-static 
        /// context into a filtered suggestionentry list for the ui
        /// </summary>
        /// <param name="fullInfo">Full type information.</param>
        /// <param name="extensionMethods">The extension methods.</param>
        /// <param name="staticAccess">if set to <c>true</c>, access is static (if false, access is instance).</param>
        /// <returns></returns>
        protected IEnumerable<CompletionEntry> MapSuggestions(TypeInformation fullInfo, IEnumerable<TypeMember> extensionMethods, bool staticAccess)
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
