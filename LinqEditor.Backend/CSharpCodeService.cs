using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Backend
{
    internal static class Foo
    {
        public static string FooTest(this string str)
        {
            return str;
        }
    }

    public class CSharpCodeService
    {
        public CSharpSyntaxNode FindEntry()
        {
            return null;
        }

        public Assembly Run(string src, string editorSrc) {
            SyntaxTree tree = CSharpSyntaxTree.ParseText(src);
            var root = (CompilationUnitSyntax)tree.GetRoot();
            
            var references = new[] {
                MetadataReference.CreateFromAssembly(typeof(System.Object).Assembly), // mscorlib.dll
                MetadataReference.CreateFromAssembly(typeof(System.ComponentModel.Component).Assembly), // System.Core.dll 4.0
                MetadataReference.CreateFromAssembly(typeof(System.Data.DataColumn).Assembly), // System.Data.dll
                MetadataReference.CreateFromAssembly(typeof(System.Xml.XmlDocument).Assembly), // System.Xml.dll
                MetadataReference.CreateFromAssembly(typeof(System.Linq.Enumerable).Assembly), // System.Core.dll 3.5 for some reason also needed?
                MetadataReference.CreateFromAssembly(typeof(IQToolkit.QueryProvider).Assembly), // IQToolkit.dll
                MetadataReference.CreateFromAssembly(typeof(IQToolkit.Data.DbEntityProvider).Assembly), // IQToolkit.Data.dll
                MetadataReference.CreateFromAssembly(typeof(IQToolkit.Data.SqlClient.TSqlLanguage).Assembly), // IQToolkit.Data.SqlClient.dll
                MetadataReference.CreateFromAssembly(typeof(LinqEditor.Backend.CSharpCodeService).Assembly) // LinqEditor.Backend.dll
            };

            var compilerOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);

            var compilation = CSharpCompilation.Create("HelloWorld")
                .AddReferences(references)
                .WithOptions(compilerOptions)
                .AddSyntaxTrees(tree);
            var semanticModel = compilation.GetSemanticModel(tree);
            var syms = semanticModel.LookupSymbols(0);

            // determines type at edit location
            var dotTextSpan = new TextSpan(src.IndexOf(editorSrc) + editorSrc.Length - 1, 1);
            var memberAccessNode = (MemberAccessExpressionSyntax)tree.GetRoot().DescendantNodes(dotTextSpan).Last();
            var lhsType = semanticModel.GetTypeInfo(memberAccessNode.Expression).Type;
            var symInfo = semanticModel.GetSymbolInfo(memberAccessNode.Expression);

            // gets types available at edit location
            var availableTypes = semanticModel
                .LookupSymbols(memberAccessNode.Span.End)
                .OfType<INamedTypeSymbol>()
                .Where(x => x.CanBeReferencedByName && x.IsStatic && !x.IsAbstract &&
                    !x.ContainingNamespace.Name.StartsWith("IQToolkit"));

            // lookup extension methods on available types
            var availableExtensionMethods = new List<IMethodSymbol>();
            foreach (var type in availableTypes)
            {
                var methods = type.GetMembers().OfType<IMethodSymbol>();
                var extensions = methods.Where(m => m.IsExtensionMethod && m.CanBeReferencedByName);
                availableExtensionMethods.AddRange(extensions);
            }

            // compile from stream and return assembly
            var stream = new MemoryStream();
            var compilationResult = compilation.Emit(stream);
            var errs = compilationResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();

            return compilationResult.Success ? Assembly.Load(stream.GetBuffer()) : null;
        }
    }
}
