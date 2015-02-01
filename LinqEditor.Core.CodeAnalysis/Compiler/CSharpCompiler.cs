using LinqEditor.Core.CodeAnalysis.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.CodeAnalysis.Compiler
{
    public class CSharpCompiler : ICSharpCompiler
    {
        public static MetadataReference[] GetStandardReferences()
        {
            return new[] {
                MetadataReference.CreateFromAssembly(typeof(System.Object).Assembly), // mscorlib.dll
                MetadataReference.CreateFromAssembly(typeof(System.ComponentModel.Component).Assembly), // System.Core.dll 4.0
                MetadataReference.CreateFromAssembly(typeof(System.Data.DataColumn).Assembly), // System.Data.dll
                MetadataReference.CreateFromAssembly(typeof(System.Xml.XmlDocument).Assembly), // System.Xml.dll
                MetadataReference.CreateFromAssembly(typeof(System.Linq.Enumerable).Assembly), // System.Core.dll 3.5 for some reason also needed?
                MetadataReference.CreateFromAssembly(typeof(IQToolkit.QueryProvider).Assembly), // IQToolkit.dll
                MetadataReference.CreateFromAssembly(typeof(IQToolkit.Data.DbEntityProvider).Assembly), // IQToolkit.Data.dll
                MetadataReference.CreateFromAssembly(typeof(IQToolkit.Data.SqlClient.TSqlLanguage).Assembly), // IQToolkit.Data.SqlClient.dll
                MetadataReference.CreateFromAssembly(typeof(LinqEditor.Common.Generated.Dumper).Assembly) // LinqEditor.Core.CodeAnalysis.dll
            };
        }


        public CompilerResult Compile(string src, string assemblyName, string outputFolder = null, byte[] reference = null)
        {
            var refs = GetStandardReferences().Concat(reference == null? new MetadataReference[]{ } : new [] { MetadataReference.CreateFromImage(reference) });
            return Compile(src, assemblyName, outputFolder, refs.ToArray());
        }

        public CompilerResult Compile(string src, string assemblyName, string outputFolder = null, params string[] otherReferences)
        {
            var refs = GetStandardReferences().Concat(otherReferences != null && otherReferences.Length > 0 ? otherReferences.Select(x => MetadataReference.CreateFromFile(x) as MetadataReference) : new MetadataReference[] { });
            return Compile(src, assemblyName, outputFolder, refs.ToArray());
        }

        private string AssemblyFilename()
        {
            var guid = Guid.NewGuid().ToString().Replace("-", "");
            return guid;
        }

        private SyntaxTree GetTree(string source, string filename)
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

        private CompilerResult Compile(string src, string assemblyName, string outputFolder, MetadataReference[] references)
        {
            var generateFiles = outputFolder != null;
            var filename = generateFiles ? outputFolder + assemblyName : null;
            var compilerOptions = new CSharpCompilationOptions(outputKind: OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Debug);
            var tree = GetTree(src, filename);

            var compilation = CSharpCompilation.Create(assemblyName)
                .WithOptions(compilerOptions)
                .AddReferences(references)
                .AddSyntaxTrees(new SyntaxTree[] { tree });

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
                Errors = compilationResult.Diagnostics.Where(e => e.Severity == DiagnosticSeverity.Error).Select(x =>
                {
                    var loc = x.Location.GetMappedLineSpan().Span;
                    return new Error
                    {
                        // errors are for display purposes, so offsetting one
                        Location = new LocationSpan
                        {
                            StartLine = loc.Start.Line+1,
                            StartColumn = loc.Start.Character+1,
                            EndLine = loc.End.Line+1,
                            EndColumn = loc.End.Character+1
                        },
                        Message = x.GetMessage()
                    };
                }),
                Warnings = compilationResult.Diagnostics.Where(w => w.Severity == DiagnosticSeverity.Warning).Select(x =>
                {
                    var loc = x.Location.GetMappedLineSpan().Span;
                    return new Warning
                    {
                        Location = new LocationSpan
                        {
                            StartLine = loc.Start.Line+1,
                            StartColumn = loc.Start.Character+1,
                            EndLine = loc.End.Line+1,
                            EndColumn = loc.End.Character+1
                        },
                        Message = x.GetMessage()
                    };
                }),
                AssemblyBytes = compilationResult.Success && !generateFiles ? stream.ToArray() : null,
                AssemblyPath = generateFiles ? filename + ".dll" : null
            };
        }

        protected virtual string GetAssemblyDirectory()
        {
            return Common.Utility.CachePath();
        }
    }
}
