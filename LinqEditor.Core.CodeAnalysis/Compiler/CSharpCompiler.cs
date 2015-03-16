using LinqEditor.Core.CodeAnalysis.Documentation;
using LinqEditor.Core.CodeAnalysis.Helpers;
using LinqEditor.Core.CodeAnalysis.Services;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace LinqEditor.Core.CodeAnalysis.Compiler
{
    public static class CSharpCompiler
    {
        public static Assembly[] GetCoreAssemblies()
        {
            return new Assembly[] 
            {
                typeof(System.Object).Assembly,// mscorlib.dll
                typeof(System.ComponentModel.Component).Assembly, // System.Core.dll 4.0
                typeof(System.Data.DataColumn).Assembly, // System.Data.dll
                typeof(System.Xml.XmlDocument).Assembly, // System.Xml.dll
                typeof(System.Linq.Enumerable).Assembly, // System.Core.dll 3.5 for some reason also needed?
            };
        }

        public static Assembly[] GetCustomAssemblies()
        {
            return new Assembly[]
            {
                typeof(IQToolkit.QueryProvider).Assembly,  // IQToolkit.dll
                typeof(IQToolkit.Data.DbEntityProvider).Assembly,  // IQToolkit.Data.dll
                typeof(IQToolkit.Data.SqlClient.TSqlLanguage).Assembly, // IQToolkit.Data.SqlClient.dll
                typeof(LinqEditor.Core.Generated.Dumper).Assembly, // LinqEditor.Core.dll
            };
        }

        private static DocumentationProvider GetDocumentationService(Assembly targetAssembly)
        {
            // this doesn't work either ...
            var asm = typeof(Microsoft.CodeAnalysis.Workspace).Assembly;
            Type providerType = null;
            DocumentationProvider providerInstance = null;
            try
            {
                var count = asm.DefinedTypes.Count();
            }
            // some types cannot be loaded, but the one we need can be resolved
            catch (ReflectionTypeLoadException exn)
            {
                providerType = exn.Types.FirstOrDefault(x => x != null &&
                    // the provider is a nested private sealed class
                    x.FullName == "Microsoft.CodeAnalysis.XmlDocumentationProvider+ContentBasedXmlDocumentationProvider");
            }
            var path = CustomDocumentationProvider.GetAssemblyDocmentationPath(targetAssembly);

            if (providerType != null && !string.IsNullOrWhiteSpace(path))
            {
                var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.CreateInstance;
                providerInstance = Activator.CreateInstance(providerType, flags, null, new object[] { File.ReadAllBytes(path) }, null) 
                    as DocumentationProvider;
            }

            return providerInstance;
        }

        public static IEnumerable<MetadataReference> GetStandardReferencesWithIncludes(string[] references)
        {
            return GetStandardReferences(includeDocumentation: false)
                .Concat(references.Select(x => MetadataReference.CreateFromFile(x)))
                .ToArray();
        }

        public static MetadataReference[] GetStandardReferences(bool includeDocumentation = true)
        {
            //var path = CustomDocumentationProvider.GetAssemblyDocmentationPath(GetCoreAssemblies().First());
            //var docReader = NuDoq.DocReader.Read(path);
            //var visitor = docReader.Accept(new DocumentationVisitor());
            var assems = new List<MetadataReference>();
            // todo: supposedly passing in CustomDocumentationProvider here would
            // cause the api to return the docs through the api (GetDocumentationCommentXml)
            // but that doesn't seem to do anything. so a workaround is to just serve the comments
            // from a custom service (DocumentationService), which builds a list of 
            // CustomDocumentationProvider by using the above GetAssemblies methods.
            return assems
                .Concat(GetCoreAssemblies().Select(x => !includeDocumentation ? MetadataReference.CreateFromAssembly(x) :
                    MetadataReference.CreateFromAssembly(x, MetadataReferenceProperties.Assembly, new CustomDocumentationProvider(x))))
                .Concat(GetCustomAssemblies().Select(x => MetadataReference.CreateFromAssembly(x)))
                .ToArray();
        }

        public static CompilerResult CompileToFile(string src, string assemblyName, string outputFolder, params object[] references)
        {
            return CompileToFile(src, false, assemblyName, outputFolder, references);
        }

        public static CompilerResult CompileToFile(string src, bool completeSource, string assemblyName, string outputFolder, params object[] references)
        {
            if (references.Where(x => !(x is string || x is byte[])).Count() > 0)
            {
                throw new ArgumentException("references had invalid types");
            }
            var refs = references.Select(x => x is string ?
                MetadataReference.CreateFromFile(x as string) : MetadataReference.CreateFromImage(x as byte[]));
            return Compile(src, assemblyName, outputFolder, completeSource, GetStandardReferences(includeDocumentation: false).Concat(refs));
        }

        public static CompilerResult CompileToBytes(string src, string assemblyName, params object[] references)
        {
            return CompileToBytes(src, false, assemblyName, references);
        }

        public static CompilerResult CompileToBytes(string src, bool completeSource, string assemblyName, params object[] references)
        {
            if (references.Where(x => !(x is string || x is byte[])).Count() > 0) 
            {
                throw new ArgumentException("references had invalid types");
            }
            var refs = references.Select(x => x is string ? 
                MetadataReference.CreateFromFile(x as string) : MetadataReference.CreateFromImage(x as byte[]));
            return Compile(src, assemblyName, null, completeSource, GetStandardReferences(includeDocumentation: false).Concat(refs));
        }

        private static CompilerResult Compile(string src, string assemblyName, string outputFolder, bool completeSource, IEnumerable<MetadataReference> references)
        {
            var generateFiles = outputFolder != null;
            var filename = generateFiles ? outputFolder + assemblyName : null;
            var compilerOptions = new CSharpCompilationOptions(outputKind: OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Debug);
            var tree = GetTree(src, filename);

            CSharpCompilation compilation = CSharpCompilation.Create(assemblyName)
                .WithOptions(compilerOptions)
                .AddReferences(references)
                .AddSyntaxTrees(new SyntaxTree[] { tree });

            if (completeSource)
            {
                var errs = CodeAnalysisHelper.GetErrors(compilation.GetDiagnostics());
                if (errs.Where(x => x.Code == "CS1002").Count() > 0 && errs.Where(x => x.Code != "CS1002").Count() == 0)
                {
                    var newSrc = CodeAnalysisHelper.CanCompleteTree(src, references);
                    var newTree = GetTree(newSrc, filename);
                    compilation = CSharpCompilation.Create(assemblyName)
                        .WithOptions(compilerOptions)
                        .AddReferences(references)
                        .AddSyntaxTrees(new SyntaxTree[] { newTree });
                }
            }

            EmitResult compilationResult = null;
            MemoryStream stream = null;

            if (!string.IsNullOrWhiteSpace(outputFolder))
            {
                using (var dllStream = new FileStream(filename + ".dll", FileMode.OpenOrCreate))
                using (var pdbStream = new FileStream(filename + ".pdb", FileMode.OpenOrCreate))
                {
                    compilationResult = compilation.Emit(peStream: dllStream,
                        pdbStream: pdbStream
                        ,
                            
                        options: new EmitOptions(pdbFilePath: filename + ".pdb", debugInformationFormat: DebugInformationFormat.Pdb)
                        );
                }
            }
            else
            {
                stream = new MemoryStream();
                compilationResult = compilation.Emit(stream, options: new EmitOptions());
            }

            return new CompilerResult
            {
                Success = compilationResult.Success,
                Errors = CodeAnalysisHelper.GetErrors(compilationResult.Diagnostics),
                Warnings = CodeAnalysisHelper.GetWarnings(compilationResult.Diagnostics),
                AssemblyBytes = compilationResult.Success && !generateFiles ? stream.ToArray() : null,
                AssemblyPath = generateFiles ? filename + ".dll" : null
            };
        }

        private static SyntaxTree GetTree(string source, string filename)
        {
            if (!string.IsNullOrWhiteSpace(filename))
            {
                // this works, but VS complains the file doesn't match somehow
                using (var file = File.CreateText(filename + ".cs"))
                {
                    file.Write(source);
                }
                return SyntaxFactory.ParseSyntaxTree(source, path: filename + ".cs", options: CSharpParseOptions.Default, encoding: Encoding.Default);
            }
            else
            {
                return CSharpSyntaxTree.ParseText(source);
            }
        }
    }
}
