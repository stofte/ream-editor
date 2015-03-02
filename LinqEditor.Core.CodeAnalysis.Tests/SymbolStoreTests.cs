using LinqEditor.Core.CodeAnalysis.Compiler;
using LinqEditor.Core.CodeAnalysis.Services;
using LinqEditor.Core.Templates;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.CodeAnalysis.Tests
{
    class TestSymbolStore : SymbolStore
    {
        public int Count { get { return _symbols.Count(); } }
    }

    [TestFixture]
    public class SymbolStoreTests
    {
        CSharpCompilationOptions _compilerOptions = new CSharpCompilationOptions(outputKind: OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Debug);

        SemanticModel _model1;
        string _source1 = @"
using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;

namespace Test
{
    public class Program
    {
        public void Query() 
        {
" + SchemaConstants.Marker + @"
        }
    }
}
";

        [TestFixtureSetUp]
        public void Initialize() 
        {
            var tree = CSharpSyntaxTree.ParseText(_source1);
            var compilation = CSharpCompilation.Create("test")
                .WithOptions(_compilerOptions)
                .AddReferences(CSharpCompiler.GetStandardReferences(includeDocumentation: false))
                .AddSyntaxTrees(new SyntaxTree[] { tree });

            _model1 = compilation.GetSemanticModel(tree);
        }

        [Test]
        public void Can_Record_And_Retrieve_Id()
        {
            var store = new SymbolStore();
            var allSymbols = _model1.LookupSymbols(_source1.IndexOf(SchemaConstants.Marker)).OfType<INamedTypeSymbol>();
            
            store.Record(allSymbols);
            var translated = store.TranslateCref(allSymbols.First().GetDocumentationCommentId());

            Assert.IsNotNullOrEmpty(translated);
        }

        [Test]
        public void Adds_Symbols_Once_Only()
        {
            var store = new TestSymbolStore();
            var allSymbols = _model1.LookupSymbols(_source1.IndexOf(SchemaConstants.Marker)).OfType<INamedTypeSymbol>();

            store.Record(allSymbols);
            var count1 = store.Count;
            store.Record(allSymbols);
            var count2 = store.Count;

            Assert.AreEqual(count1, count2);
        }
    }
}
