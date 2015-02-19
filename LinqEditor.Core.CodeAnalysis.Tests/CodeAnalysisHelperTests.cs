using LinqEditor.Core.CodeAnalysis.Compiler;
using LinqEditor.Core.CodeAnalysis.Helpers;
using LinqEditor.Core.Templates;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using NUnit.Framework;
using System;
using System.Linq;

namespace LinqEditor.Core.CodeAnalysis.Tests
{
    [TestFixture]
    public class CodeAnalysisHelperTests
    {
        SemanticModel _model1;
        SyntaxTree _tree1;
        string _source1 = @"
using LinqEditor.Core.Generated;

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

        SemanticModel _model2;
        SyntaxTree _tree2;
        string _source2;
        string _sourceStub2 = @"int x = 0x2a; x.";
        int _stubOffset;


        SemanticModel _model3;
        SyntaxTree _tree3;
        string _source3 = @"
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
            var compilerOptions = new CSharpCompilationOptions(outputKind: OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Debug);

            // template with universal type extension method
            _tree1 = CSharpSyntaxTree.ParseText(_source1);
            var compilation1 = CSharpCompilation.Create("comp1")
                .WithOptions(compilerOptions)
                .AddReferences(CSharpCompiler.GetStandardReferences())
                .AddSyntaxTrees(new SyntaxTree[] { _tree1 });

            _model1 = compilation1.GetSemanticModel(_tree1);


            // instance program
            _source2 = _source1.Replace(SchemaConstants.Marker, _sourceStub2);
            _tree2 = CSharpSyntaxTree.ParseText(_source2);
            var compilation2 = CSharpCompilation.Create("comp2")
                .WithOptions(compilerOptions)
                .AddReferences(CSharpCompiler.GetStandardReferences())
                .AddSyntaxTrees(new SyntaxTree[] { _tree2 });
            _model2 = compilation2.GetSemanticModel(_tree2);
            _stubOffset = _source1.IndexOf(SchemaConstants.Marker) + _sourceStub2.Length - 1;


            // plain template
            _tree3 = CSharpSyntaxTree.ParseText(_source3);
            var compilation3 = CSharpCompilation.Create("comp3")
                .WithOptions(compilerOptions)
                .AddReferences(CSharpCompiler.GetStandardReferences())
                .AddSyntaxTrees(new SyntaxTree[] { _tree3 });

            var warns = CodeAnalysisHelper.GetErrors(compilation3.GetDiagnostics());
            _model3 = compilation3.GetSemanticModel(_tree3);
        }

        [Test]
        public void Universal_Type_Is_Returned_With_Key_As_Asterisks()
        {
            var methods = CodeAnalysisHelper.GetExtensionMethods(_model1);
            Assert.AreEqual(methods.Count, 1);
            Assert.IsTrue(methods.ContainsKey(CodeAnalysisHelper.UniversalTypeKey));
        }

        [Test]
        public void Universal_Type_Key_Is_Always_Returned()
        {
            var methods = CodeAnalysisHelper.GetExtensionMethods(_model3);
            Assert.GreaterOrEqual(methods.Count, 1); // must contain at least the universal key
            Assert.IsTrue(methods.ContainsKey(CodeAnalysisHelper.UniversalTypeKey));
        }

        [Test]
        public void Returns_Universal_Type_Extension_Methods_When_Querying_Int()
        {
            var dotTextSpan = new TextSpan(_stubOffset, 1);
            var syntaxNode = _tree2.GetRoot()
                    .DescendantNodes(dotTextSpan)
                    .OfType<MemberAccessExpressionSyntax>()
                    .LastOrDefault();

            var typeInfo = _model2.GetTypeInfo(syntaxNode.Expression);
            var symInfo = _model2.GetSymbolInfo(syntaxNode.Expression);

            // initialize dict with the template
            var methods = CodeAnalysisHelper.GetExtensionMethods(_model1);

            // map extensions to specific type
            var extensions = CodeAnalysisHelper.GetTypeExtensionMethods(typeInfo, methods);

            Assert.AreEqual(extensions.Count(), 1);
            Assert.AreEqual(extensions.Single().Name, "Dump");
        }
    }
}
