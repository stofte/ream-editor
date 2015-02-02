//using LinqEditor.Core.CodeAnalysis.Compiler;
//using LinqEditor.Core.CodeAnalysis.Repositories;
//using LinqEditor.Core.Context;
//using LinqEditor.Core.Templates;
//using LinqEditor.Core.Helpers;
//using Microsoft.CodeAnalysis.CSharp;
//using NUnit.Framework;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.CodeAnalysis;

//namespace LinqEditor.Core.CodeAnalysis.Tests
//{
//    [TestFixture]
//    public class TypeInformationStoreTests
//    {
//        IContext _context;
//        string _assemblyPath;
//        string _source1 = @"
//using System;
//using System.Linq;
//using System.Collections.Generic;
//
//namespace TypeTest 
//{
//    public class Program
//    {
//        public string Execute() 
//        {
//"+ SchemaConstants.Marker  +@"
//        }
//    }
//}";

//        [TestFixtureSetUp]
//        public void Initialize()
//        {
//            var result = CSharpCompiler.CompileToFile(_source1, "TypeTest", PathUtility.TempPath);
//            _assemblyPath = result.AssemblyPath;
//            _context = new Context.Context();
//        }

//        [TestFixtureTearDown]
//        public void Cleanup()
//        {
//            var file = PathUtility.TempPath + "TypeTest.dll";
//            if (File.Exists(file))
//            {
//                File.Delete(file);
//            }
//        }

//        [Test]
//        public void Can_Construct_TypeInformationStore_And_Initialize_Model()
//        {
            
//            var tree = CSharpSyntaxTree.ParseText(_source1);
//            var compilation = CSharpCompilation.Create(Guid.NewGuid().ToIdentifierWithPrefix("d"))
//                .AddReferences(CSharpCompiler.GetStandardReferences())
//                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
//                .AddSyntaxTrees(tree);

//            var model = compilation.GetSemanticModel(tree);
//            var store = new TypeInformationStore();
//            store.InitializeModel(model, tree);
//        }

//        [Test]
//        public void Can_Return_TypeInformation_For_Model()
//        {
//            var tree = CSharpSyntaxTree.ParseText(_source1);
//            var compilation = CSharpCompilation.Create(Guid.NewGuid().ToIdentifierWithPrefix("d"))
//                .AddReferences(CSharpCompiler.GetStandardReferences())
//                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
//                .AddSyntaxTrees(tree);

//            var model = compilation.GetSemanticModel(tree);
//            var store = new TypeInformationStore();
//            store.InitializeModel(model, tree);

//            // create new source, parse it out, and attempt to retrieve type

//            var dotTextSpan = new TextSpan(_sourceOffset + fragmentIndex, 1);
//            var memberAccessNode = (MemberAccessExpressionSyntax)tree.GetRoot().DescendantNodes(dotTextSpan).Last();

//            var lhs = model.GetTypeInfo(memberAccessNode.Expression);
//            var symInfo = model.GetSymbolInfo(memberAccessNode.Expression);


//        }
//    }
//}
