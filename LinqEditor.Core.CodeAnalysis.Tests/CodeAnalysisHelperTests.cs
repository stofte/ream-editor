using LinqEditor.Core.CodeAnalysis.Compiler;
using LinqEditor.Core.Helpers;
using LinqEditor.Core.CodeAnalysis.Helpers;
using LinqEditor.Core.Templates;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using NUnit.Framework;
using System;
using System.Linq;
using LinqEditor.Test.Common;
using System.Text;
using LinqEditor.Core.CodeAnalysis.Services;

namespace LinqEditor.Core.CodeAnalysis.Tests
{
    [TestFixture]
    public class CodeAnalysisHelperTests
    {
        IDocumentationService _documentationService;
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

        int _stubOffset4;
        string _source4 = @"
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
        CSharpCompilationOptions _compilerOptions = new CSharpCompilationOptions(outputKind: OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Debug);

        [TestFixtureSetUp]
        public void Initialize()
        {
            // template with universal type extension method
            _tree1 = CSharpSyntaxTree.ParseText(_source1);
            var compilation1 = CSharpCompilation.Create("comp1")
                .WithOptions(_compilerOptions)
                .AddReferences(CSharpCompiler.GetStandardReferences(includeDocumentation: false))
                .AddSyntaxTrees(new SyntaxTree[] { _tree1 });

            _model1 = compilation1.GetSemanticModel(_tree1);


            // instance program
            _source2 = _source1.Replace(SchemaConstants.Marker, _sourceStub2);
            _tree2 = CSharpSyntaxTree.ParseText(_source2);
            var compilation2 = CSharpCompilation.Create("comp2")
                .WithOptions(_compilerOptions)
                .AddReferences(CSharpCompiler.GetStandardReferences(includeDocumentation: false))
                .AddSyntaxTrees(new SyntaxTree[] { _tree2 });
            _model2 = compilation2.GetSemanticModel(_tree2);
            _stubOffset = _source1.IndexOf(SchemaConstants.Marker) + _sourceStub2.Length - 1;


            // plain template
            _tree3 = CSharpSyntaxTree.ParseText(_source3);
            var compilation3 = CSharpCompilation.Create("comp3")
                .WithOptions(_compilerOptions)
                .AddReferences(CSharpCompiler.GetStandardReferences(includeDocumentation: false))
                .AddSyntaxTrees(new SyntaxTree[] { _tree3 });

            var warns = CodeAnalysisHelper.GetErrors(compilation3.GetDiagnostics());
            _model3 = compilation3.GetSemanticModel(_tree3);

            // full template
            _stubOffset4 = _source4.IndexOf(SchemaConstants.Marker);

            _documentationService = new DocumentationService();
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

        [TestCase(VSDocumentationTestData.VarDeclOfIntAtZero)]
        [TestCase(VSDocumentationTestData.VarDeclOfIntHashSetAtZero)]
        [TestCase(VSDocumentationTestData.FullDeclOfDataColumnAtZero)]
        [TestCase(VSDocumentationTestData.VarDeclOfQueryableAtZero)]
        [TestCase(VSDocumentationTestData.FullDeclOfMultipleIntsAtZero)]
        [TestCase(VSDocumentationTestData.VarDeclOfIntListAtZero)]
        public void GetToolTipDisplayName_Formats_Type_Correctly(string testDataKey)
        {
            var toolTipTestData = VSDocumentationTestData.Data[testDataKey];
            var docData = toolTipTestData.Item3;
            var source = _source4.Replace(SchemaConstants.Marker, toolTipTestData.Item1);
            var tree = CSharpSyntaxTree.ParseText(source);

            var dotTextSpan = new TextSpan(_stubOffset4 + toolTipTestData.Item2, 1);
            var syntaxNode = tree.GetRoot()
                    .DescendantNodes(dotTextSpan)
                    .OfType<VariableDeclarationSyntax>()
                    .LastOrDefault();

            var compilation = CSharpCompilation.Create(Guid.NewGuid().ToIdentifierWithPrefix("test"))
                .WithOptions(_compilerOptions)
                .AddReferences(CSharpCompiler.GetStandardReferences(includeDocumentation: false))
                .AddSyntaxTrees(new SyntaxTree[] { tree });

            var semanticModel = compilation.GetSemanticModel(tree);

            var errors = CodeAnalysisHelper.GetErrors(compilation.GetDiagnostics());
            var typeInfo = semanticModel.GetTypeInfo(syntaxNode.Type);
            var symInfo = semanticModel.GetSymbolInfo(syntaxNode.Type);
            
            var actualOutput = CodeAnalysisHelper.GetDisplayNameAndSpecializations(typeInfo, symInfo);

            Assert.AreEqual(docData.Item1, actualOutput.Item1);
            Assert.AreEqual(docData.Item3, actualOutput.Item2);
        }

        [TestCase(SourceCodeFragments.ErrorExample1)]
        [TestCase(SourceCodeFragments.ErrorExample2)]
        public void Errors_Are_Returned_With_Line_Column_And_Index_Indicators(string sourceStub)
        {
            var source = _source4.Replace(SchemaConstants.Marker, sourceStub);
            var tree = CSharpSyntaxTree.ParseText(source);

            var compilation = CSharpCompilation.Create(Guid.NewGuid().ToIdentifierWithPrefix("test"))
                .WithOptions(_compilerOptions)
                .AddReferences(CSharpCompiler.GetStandardReferences(includeDocumentation: false))
                .AddSyntaxTrees(new SyntaxTree[] { tree });

            var semanticModel = compilation.GetSemanticModel(tree);

            var errors = CodeAnalysisHelper.GetErrors(compilation.GetDiagnostics());

            foreach(var err in errors)
            {
                var byIndex = source.Substring(err.Location.StartIndex, err.Location.EndIndex - err.Location.StartIndex);
                var byLineColumn = err.Location.GetText(source);

                Assert.AreEqual(byIndex, byLineColumn);
            }
        }

        [TestCase(VSDocumentationTestData.FullDeclerationOfIntAtZero)]
        [TestCase(VSDocumentationTestData.FullDeclOfDataColumnAtZero)]
        [TestCase(VSDocumentationTestData.FullDeclOfMultipleIntsAtZero)]
        [TestCase(VSDocumentationTestData.VarDeclOfIntAtZero)]
        [TestCase(VSDocumentationTestData.VarDeclOfIntHashSetAtZero)]
        [TestCase(VSDocumentationTestData.VarDeclOfIntListAtZero)]
        [TestCase(VSDocumentationTestData.VarDeclOfQueryableAtZero)]
        //[TestCase(VSDocumentationTestData.VarDeclOfQueryableAtTen)]
        public void Formats_ToolTip_Correctly(string testDataKey)
        {
            var toolTipTestData = VSDocumentationTestData.Data[testDataKey];
            var docData = toolTipTestData.Item3;
            var source = _source4.Replace(SchemaConstants.Marker, toolTipTestData.Item1);
            var tree = CSharpSyntaxTree.ParseText(source);

            var dotTextSpan = new TextSpan(_stubOffset4 + toolTipTestData.Item2, 1);
            var nodes = tree.GetRoot()
                    .DescendantNodes(dotTextSpan);

            var compilation = CSharpCompilation.Create(Guid.NewGuid().ToIdentifierWithPrefix("test"))
                .WithOptions(_compilerOptions)
                .AddReferences(CSharpCompiler.GetStandardReferences(includeDocumentation: false))
                .AddSyntaxTrees(new SyntaxTree[] { tree });

            var semanticModel = compilation.GetSemanticModel(tree);
            var errors = CodeAnalysisHelper.GetErrors(compilation.GetDiagnostics());

            var tip = CodeAnalysisHelper.GetToolTip(nodes, semanticModel, _documentationService);

            Assert.AreEqual(docData.Item1, tip.TypeAndName);
            Assert.AreEqual(docData.Item2, tip.Description);
            Assert.AreEqual(docData.Item3, tip.Specializations);
        }
    }
}
