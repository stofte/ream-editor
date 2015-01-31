using LinqEditor.Core.CodeAnalysis.Compiler;
using LinqEditor.Core.Templates;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.CodeAnalysis.Services
{
    public class Editor : IEditor
    {
        private MetadataReference[] _references;
        private ITemplateService _templateService;
        private string _initialSource;
        private string _currentSource;
        private int _sourceOffset;
        private string _entryNamespace;
        private string _entryName;

        public Editor(ITemplateService templateService)
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
            _currentSource = _initialSource = _templateService.GenerateQuery(Guid.NewGuid(), out queryNamespace, SchemaConstants.Marker, schemaNamespace);
            var tree = CSharpSyntaxTree.ParseText(_initialSource);
            var semanticModel = GetModel(tree);
            _sourceOffset = _initialSource.IndexOf(SchemaConstants.Marker);

            // find entry point
            var nodes = tree.GetRoot().DescendantNodes();
            // test code has more then one entry
            var entryMethod = nodes.OfType<MethodDeclarationSyntax>().Last();
            _entryNamespace = nodes.OfType<NamespaceDeclarationSyntax>().Last().Name.ToString();
            _entryName = nodes.OfType<MethodDeclarationSyntax>().Last().Identifier.ValueText;
        }

        public Models.EditContext UpdateSource(string sourceFragment, int updateIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Models.SuggestionEntry> MemberCompletions()
        {
            throw new NotImplementedException();
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
    }
}
