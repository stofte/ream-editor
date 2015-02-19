using LinqEditor.Core.CodeAnalysis.Compiler;
using LinqEditor.Core.CodeAnalysis.Helpers;
using LinqEditor.Core.Templates;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using System;

namespace LinqEditor.Core.CodeAnalysis.Tests
{
    [TestFixture]
    public class CodeAnalysisHelperTests
    {
        SemanticModel _model1;
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

        [TestFixtureSetUp]
        public void Initialize()
        {
            var tree1 = CSharpSyntaxTree.ParseText(_source1);
            var compilerOptions = new CSharpCompilationOptions(outputKind: OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Debug);
            var compilation = CSharpCompilation.Create("comp1")
                .WithOptions(compilerOptions)
                .AddReferences(CSharpCompiler.GetStandardReferences())
                .AddSyntaxTrees(new SyntaxTree[] { tree1 });

            _model1 = compilation.GetSemanticModel(tree1);
        }

        [Test]
        public void Universal_Type_Is_Returned_With_Key_As_Asterisks()
        {
            var methods = CodeAnalysisHelper.GetExtensionMethods(_model1);
            Assert.AreEqual(methods.Count, 1);
            Assert.IsTrue(methods.ContainsKey(CodeAnalysisHelper.UniversalTypeKey));

        }
    }
}
