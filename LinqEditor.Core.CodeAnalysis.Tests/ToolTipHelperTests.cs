using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using LinqEditor.Core.CodeAnalysis.Compiler;
using LinqEditor.Core.CodeAnalysis.Helpers;
using LinqEditor.Core.CodeAnalysis.Services;
using LinqEditor.Core.Templates;
using LinqEditor.Test.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace LinqEditor.Core.CodeAnalysis.Tests
{
    [TestFixture]
    public class ToolTipHelperTests
    {
        IWindsorContainer _container;

        CSharpCompilationOptions _compilerOptions = new CSharpCompilationOptions(outputKind: OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Debug);
        IDocumentationService _documentationService;
        int _offset1;
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

        IToolTipHelperFactory _factory;

        [TestFixtureSetUp]
        public void Initialize()
        {
            // full template
            _offset1 = _source1.IndexOf(SchemaConstants.Marker);
            var tree1 = CSharpSyntaxTree.ParseText(_source1);
            // need to compile to get symbol list
            var compilation1 = CSharpCompilation.Create("comp1")
                            .WithOptions(_compilerOptions)
                            .AddReferences(CSharpCompiler.GetStandardReferences(includeDocumentation: false))
                            .AddSyntaxTrees(new SyntaxTree[] { tree1 });

            var symbolStore = new SymbolStore();
            var model = compilation1.GetSemanticModel(tree1);
            symbolStore.Record(model.LookupSymbols(_source1.IndexOf(SchemaConstants.Marker)).OfType<INamedTypeSymbol>());
            _documentationService = new DocumentationService(symbolStore);

            // init container
            _container = new WindsorContainer();
            _container.AddFacility<TypedFactoryFacility>();
            _container.Register(Component.For<IToolTipHelper>()
                                         .ImplementedBy<ToolTipHelper>()
                                         .LifestyleTransient());
            _container.Register(Component.For<IDocumentationService>()
                                         .Instance(_documentationService));
            _container.Register(Component.For<IToolTipHelperFactory>().AsFactory());
        }

        [TestCase(ToolTipTestData.VarDeclOfInt_InitialDecl)]
        [TestCase(ToolTipTestData.VarDeclOfIntHashSet_InitialDecl)]
        [TestCase(ToolTipTestData.FullDeclOfDataColumn_InitialDecl)]
        [TestCase(ToolTipTestData.VarDeclOfDictionaryWithTuples_InitialDecl)]
        [TestCase(ToolTipTestData.VarDeclOfIntHashSet_Ctor)]
        [TestCase(ToolTipTestData.VarDeclOfListWithCopyFromCtor_Ctor)]
        [TestCase(ToolTipTestData.FullDeclOfDataColumn_Ctor)]
        [TestCase(ToolTipTestData.VarDeclOfIntHashSet_IntGenericTypeParam)]
        [TestCase(ToolTipTestData.VarDeclOfDictionaryWithTuples_TupleGenericTypeParam)]
        [TestCase(ToolTipTestData.FullDeclOfDataColumn_MemberProperty)]
        public void Formats_Expected_ToolTip(string testDataKey)
        {
            var testData = ToolTipTestData.Data[testDataKey];
            // full template
            var src = _source1.Replace(SchemaConstants.Marker, testData.Item1);
            var offset = _source1.IndexOf(SchemaConstants.Marker);
            var tree = CSharpSyntaxTree.ParseText(src);
            // need to compile to get symbol list
            var compilation = CSharpCompilation.Create("formatcomp")
                            .WithOptions(_compilerOptions)
                            .AddReferences(CSharpCompiler.GetStandardReferences(includeDocumentation: false))
                            .AddSyntaxTrees(new SyntaxTree[] { tree });

            var model = compilation.GetSemanticModel(tree);
            var factory = _container.Resolve<IToolTipHelperFactory>();
            var helper = factory.Create(model);

            var expected = testData.Item3;
            var actual = helper.GetToolTip(testData.Item2 + offset);

            Assert.AreEqual(expected.ItemName, actual.ItemName);
            Assert.AreEqual(expected.Description, actual.Description);
            Assert.AreEqual(expected.Addendums, actual.Addendums);
            _container.Release(factory);
        }
    }
}
