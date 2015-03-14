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
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace LinqEditor.Core.CodeAnalysis.Services
{
    public class TemplateCodeAnalysis : ITemplateCodeAnalysis
    {
        // instance data
        protected IToolTipHelperFactory _tooltipFactory;
        protected IDocumentationService _documentationService;
        protected ISymbolStore _symbolStore;
        protected ITemplateService _templateService;
        protected MetadataReference[] _references;
        protected ExtensionMethodCollection _extensionMethods;
        protected IEnumerable<INamedTypeSymbol> _availableSymbols;
        
        protected bool _initialized;
        protected string _initialSource;
        public int IndexOffset { get; set; }
        public int LineOffset { get; set; }
        protected string _namespace;
        protected string _entryClass;
        protected string _schemaNamespace;

        // current analysis model
        protected string _currentStub;
        protected IEnumerable<Warning> _warnings;
        protected IEnumerable<Error> _errors;
        protected SyntaxNode _currentTree;
        protected SemanticModel _currentModel;


        public TemplateCodeAnalysis(ITemplateService templateService, IDocumentationService documentationService, ISymbolStore symbolStore, IToolTipHelperFactory tooltipFactory)
        {
            DebugLogger.Log(GetHashCode());
            _initialized = false;
            _templateService = templateService;
            _documentationService = documentationService;
            _symbolStore = symbolStore;
            _tooltipFactory = tooltipFactory;
            _references = CSharpCompiler.GetStandardReferences(includeDocumentation: false);
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
                _schemaNamespace = schemaNamespace;
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
            var result = GetModelAndDiagnostics(tree);
            var semanticModel = result.Item1;
            _warnings = result.Item2;
            _errors = result.Item3;
            _extensionMethods = CodeAnalysisHelper.GetExtensionMethods(semanticModel);
            // assumes marker is at column 0
            var lines = _initialSource.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            for (var i = 0; i < lines.Length; i++) 
            {
                if (lines[i].StartsWith(SchemaConstants.Marker))
                {
                    LineOffset = i;
                    break;
                }
            }
            IndexOffset = _initialSource.IndexOf(SchemaConstants.Marker);
            _initialized = true;

            var nodes = tree.GetRoot().DescendantNodes();
            var nsNode = nodes.OfType<NamespaceDeclarationSyntax>().LastOrDefault();
            var clsNode = nodes.OfType<ClassDeclarationSyntax>().LastOrDefault();
            
            // cache symbols for documentation usage
            _symbolStore.Record(semanticModel.LookupSymbols(IndexOffset).OfType<INamedTypeSymbol>());

            _availableSymbols = semanticModel.LookupSymbols(IndexOffset)
                .Where(x => x.CanBeReferencedByName).OfType<INamedTypeSymbol>();
            _namespace = nsNode.Name.ToString();
            _entryClass = clsNode.Identifier.ToString();
        }

        public bool IsReady { get { return _initialized; } }
        
        public AnalysisResult Analyze(string sourceFragment)
        {
            if (sourceFragment == null) throw new ArgumentNullException("sourceFragment");

            if (_currentStub != sourceFragment)
            {
                UpdateModel(sourceFragment);
            }

            return new AnalysisResult
            {
                Errors = _errors,
                Warnings = _warnings,
                Context = UserContext.Unknown
            };
        }


        public AnalysisResult Analyze(string sourceFragment, int updateIndex)
        {
            if (sourceFragment == null) throw new ArgumentNullException("sourceFragment");

            if (_currentStub != sourceFragment)
            {
                UpdateModel(sourceFragment);
            }

            var ctx = UserContext.Unknown;
            IEnumerable<CompletionEntry> suggestions = new List<CompletionEntry>();
            ToolTipData tooltip = new ToolTipData();
            
            var oneCharTextSpan = new TextSpan(IndexOffset + updateIndex, 1);
            var nodes = _currentTree.DescendantNodes(oneCharTextSpan);
            var allNodes = _currentTree.DescendantNodes().OfType<CSharpSyntaxNode>();

            if (!string.IsNullOrWhiteSpace(_currentStub) && 
                updateIndex < _currentStub.Length &&
                _currentStub[updateIndex] == '.')
            {
                // check if context is really member access
                var syntaxNode = nodes.OfType<MemberAccessExpressionSyntax>().LastOrDefault();
                
                if (syntaxNode != null && syntaxNode.Expression != null)
                {
                    var clsNode = allNodes.OfType<ClassDeclarationSyntax>().LastOrDefault();
                    var nsNode = allNodes.OfType<NamespaceDeclarationSyntax>().LastOrDefault();

                    // used for filtering the this object specially
                    var entryClass = string.Format("{0}.{1}", nsNode.Name, clsNode.Identifier);

                    var typeInfo = _currentModel.GetTypeInfo(syntaxNode.Expression);
                    var symInfo = _currentModel.GetSymbolInfo(syntaxNode.Expression);

                    if (symInfo.Symbol == null) goto exit;
                    // should be ok to work with symbols
                    ctx = UserContext.MemberCompletion;
                    // get all applicable extensions
                    var extensions = CodeAnalysisHelper.GetTypeExtensionMethods(typeInfo.Type, _extensionMethods);
                    TypeInformation mainType = CodeAnalysisHelper.GetTypeInformation(typeInfo.Type as INamedTypeSymbol, entryClass, _schemaNamespace);
                    TypeInformation subType = null;
                    var isStatic = symInfo.Symbol.IsStatic || symInfo.Symbol.Kind == SymbolKind.NamedType;

                    if (mainType.EntryClass)
                    {
                        subType = CodeAnalysisHelper.GetTypeInformation(typeInfo.Type.BaseType, entryClass);

                        suggestions = MapSuggestions(new TypeInformation
                        {
                            EntryClass = true,
                            Name = mainType.Name,
                            Namespace = mainType.Namespace,
                            Members = subType.Members.Where(x => 
                                new [] {MemberKind.Property, MemberKind.DatabaseTable, MemberKind.TableColumn}.Contains(x.Kind))
                        }, new List<TypeMember>(), isStatic);
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
                //tooltip = CodeAnalysisHelper.GetToolTip(nodes, _currentModel, _documentationService);
                if (!string.IsNullOrWhiteSpace(tooltip.ItemName))
                {
                    ctx = UserContext.ToolTip;
                }
            }

            exit:

            return new AnalysisResult
            {
                Context = ctx,
                MemberCompletions = suggestions.OrderBy(x => x.Value).ThenBy(x => x.Kind),
                Errors = _errors,
                Warnings = _warnings,
                ToolTip = tooltip
            };
        }

        protected void UpdateModel(string sourceFragment)
        {
            _currentStub = sourceFragment;
            var currentSource = _initialSource.Replace(SchemaConstants.Marker, _currentStub);
            var tree = CSharpSyntaxTree.ParseText(currentSource);
            var result = GetModelAndDiagnostics(tree);
            _currentModel = result.Item1;
            _warnings = result.Item2;
            _errors = result.Item3;
            _currentTree = tree.GetRoot();
        }


        protected Tuple<SemanticModel, IEnumerable<Warning>, IEnumerable<Error>> GetModelAndDiagnostics(SyntaxTree tree)
        {
            var compilerOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
            var comp = CSharpCompilation.Create(Guid.NewGuid().ToIdentifierWithPrefix(SchemaConstants.QueryPrefix))
                .AddReferences(_references)
                .WithOptions(compilerOptions)
                .AddSyntaxTrees(tree);

            var diag = comp.GetDiagnostics();
            var warnings = CodeAnalysisHelper.GetWarnings(diag)
                .Select(x => new Warning 
                { 
                    Location = x.Location.Offset(-IndexOffset, -LineOffset), 
                    Message = x.Message 
                });
            var errors = CodeAnalysisHelper.GetErrors(diag)
                .Select(x => new Error 
                { 
                    Location = x.Location.Offset(-IndexOffset, -LineOffset), 
                    Message = x.Message 
                });
            return Tuple.Create(comp.GetSemanticModel(tree), warnings, errors);
            //return comp.GetSemanticModel(tree);
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
