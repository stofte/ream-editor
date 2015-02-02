using LinqEditor.Core.CodeAnalysis.Compiler;
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
    public class Editor : IEditor
    {
        private MetadataReference[] _references;
        private ITemplateService _templateService;
        private IContext _context;
        private string _initialSource;
        private string _currentSource;
        private int _sourceOffset;

        private TypeInfo _memberAccessType;
        private SymbolInfo _memberAccessSymbol;

        public Editor(ITemplateService templateService, IContext context)
        {
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
            _sourceOffset = _initialSource.IndexOf(SchemaConstants.Marker);
        }

        public EditContext UpdateSource(string sourceFragment, int updateIndex)
        {
            if (sourceFragment[updateIndex] == '.')
            {
                // check if context is really member access
                _currentSource = _initialSource.Replace(SchemaConstants.Marker, sourceFragment);
                
                var tree = CSharpSyntaxTree.ParseText(_currentSource);
                var semanticModel = GetModel(tree);
                var dotTextSpan = new TextSpan(_sourceOffset + updateIndex, 1);
                var syntaxNode = tree.GetRoot().DescendantNodes(dotTextSpan).Last();
                var ctx = syntaxNode is MemberAccessExpressionSyntax ? EditContext.MemberCompletion : EditContext.Unknown;

                if (ctx == EditContext.MemberCompletion)
                {
                    var node = syntaxNode as MemberAccessExpressionSyntax;
                    _memberAccessType = semanticModel.GetTypeInfo(node.Expression);
                    _memberAccessSymbol = semanticModel.GetSymbolInfo(node.Expression);
                }

                return ctx;
            }
            return EditContext.Unknown;
        }

        public IEnumerable<SuggestionEntry> MemberCompletions()
        {
            throw new NotImplementedException();
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
    }
}
