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
            return Compile(src, assemblyName, outputFolder, completeSource, CompilerReferences.GetStandardReferences(includeDocumentation: false).Concat(refs));
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
            return Compile(src, assemblyName, null, completeSource, CompilerReferences.GetStandardReferences(includeDocumentation: false).Concat(refs));
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

            // this probably belonds elsewhere, but this is rather convinient
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
                File.WriteAllText(filename + ".cs", source);
                return SyntaxFactory.ParseSyntaxTree(source, path: filename + ".cs", options: CSharpParseOptions.Default, encoding: Encoding.Default);
            }
            else
            {
                return CSharpSyntaxTree.ParseText(source);
            }
        }
    }
}
