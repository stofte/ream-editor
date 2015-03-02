using LinqEditor.Core.CodeAnalysis.Compiler;
using LinqEditor.Core.CodeAnalysis.Services;
using LinqEditor.Core.Templates;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LinqEditor.Core.CodeAnalysis.Tests
{
    [TestFixture]
    public class DocumentationServiceTests
    {
        IDocumentationService _service;
        ISymbolStore _realSymbolStore;

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
            var mockStore = new Mock<ISymbolStore>();

            var tree = CSharpSyntaxTree.ParseText(_source1);
            var compilation = CSharpCompilation.Create("test")
                .WithOptions(_compilerOptions)
                .AddReferences(CSharpCompiler.GetStandardReferences(includeDocumentation: false))
                .AddSyntaxTrees(new SyntaxTree[] { tree });

            _model1 = compilation.GetSemanticModel(tree);

            var store = new SymbolStore();
            store.Record(_model1.LookupSymbols(_source1.IndexOf(SchemaConstants.Marker)).OfType<INamedTypeSymbol>());
            _realSymbolStore = store;
            _service = new DocumentationService(store);
        }

        [TestCase("T:System.Int32", @"
<member name=""T:System.Int32"">
  <summary>Represents a 32-bit signed integer.</summary>
  <filterpriority>1</filterpriority>
</member>
")]
        [TestCase("T:System.Collections.Generic.HashSet`1", @"
    <member name=""T:System.Collections.Generic.HashSet`1"">
      <summary>Represents a set of values.</summary>
      <typeparam name=""T"">The type of elements in the hash set.</typeparam>
    </member>
")]
        public void Can_Return_Documentation_For_Members(string memberName, string expectedDocumentation)
        {
            return;
            var doc = _service.GetDocumentation(memberName);

            Assert.AreEqual(doc.ToString(), XElement.Parse(expectedDocumentation).ToString());
        }

        [Test]
        public void Can_Return_Documentation_For_Member()
        {
            var expected = "Initializes a new instance of the System.Collections.Generic.List<T> class that is empty and has the default initial capacity.";
            var ctorId = "M:System.Collections.Generic.List`1.#ctor";
            var x = _service.GetDocs(ctorId);

            Assert.AreEqual(x, expected);
        }
    }
}
