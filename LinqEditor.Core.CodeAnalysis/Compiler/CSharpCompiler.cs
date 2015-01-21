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
        MetadataReference[] References;

        public CSharpCompiler()
        {
            References = new[] {
                MetadataReference.CreateFromAssembly(typeof(System.Object).Assembly), // mscorlib.dll
                MetadataReference.CreateFromAssembly(typeof(System.ComponentModel.Component).Assembly), // System.Core.dll 4.0
                MetadataReference.CreateFromAssembly(typeof(System.Data.DataColumn).Assembly), // System.Data.dll
                MetadataReference.CreateFromAssembly(typeof(System.Xml.XmlDocument).Assembly), // System.Xml.dll
                // todo: check if still needed
                MetadataReference.CreateFromAssembly(typeof(System.Linq.Enumerable).Assembly), // System.Core.dll 3.5 for some reason also needed?
                MetadataReference.CreateFromAssembly(typeof(IQToolkit.QueryProvider).Assembly), // IQToolkit.dll
                MetadataReference.CreateFromAssembly(typeof(IQToolkit.Data.DbEntityProvider).Assembly), // IQToolkit.Data.dll
                MetadataReference.CreateFromAssembly(typeof(IQToolkit.Data.SqlClient.TSqlLanguage).Assembly), // IQToolkit.Data.SqlClient.dll
                MetadataReference.CreateFromAssembly(typeof(LinqEditor.Utility.Helpers.Dumper).Assembly) // LinqEditor.Core.CodeAnalysis.dll
            };
        }

        private string AssemblyFilename()
        {
            var guid = Guid.NewGuid().ToString().Replace("-", "");
            return guid;
        }

        public CompilerResult Compile(string src, string assemblyName, bool generateFiles, byte[] reference = null)
        {
            var refs = References.Concat(new[] { MetadataReference.CreateFromImage(reference) });
            return Compile(src, assemblyName, generateFiles, refs.ToArray());
        }

        public CompilerResult Compile(string src, string assemblyName, bool generateFiles, params string[] otherReferences)
        {
            var refs = References.Concat(otherReferences.Length > 0 ? otherReferences.Select(x => MetadataReference.CreateFromFile(x) as MetadataReference) : new MetadataReference[] { });
            return Compile(src, assemblyName, generateFiles, refs.ToArray());
        }


        private CompilerResult Compile(string src, string assemblyName, bool generateFiles, MetadataReference[] references)
        {
            try
            {
                var compilerOptions = new CSharpCompilationOptions(outputKind: OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Debug);
                var compilation = CSharpCompilation.Create(assemblyName, new[] { CSharpSyntaxTree.ParseText(src) }, references, compilerOptions);

                // generate dll, assemblies generated in memory by roslyn,
                // does not seem loadable for queries trying to load schema dependency
                var filename = AppDomain.CurrentDomain.BaseDirectory + "gendlls\\" + assemblyName;

                EmitResult compilationResult = null;
                MemoryStream stream = null;

                if (generateFiles)
                {

                    using (var dllStream = new FileStream(filename + ".dll", FileMode.OpenOrCreate))
                    using (var pdbStream = new FileStream(filename + ".pdb", FileMode.OpenOrCreate))
                    {
                        compilationResult = compilation.Emit(peStream: dllStream,
                            pdbStream: pdbStream,
                            options: new EmitOptions(pdbFilePath: filename + ".pdb")
                            );
                    }

                    using (var file = File.CreateText(filename + ".cs"))
                    {
                        file.Write(src);
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
                            Location = new LocationSpan
                            {
                                StartLine = loc.Start.Line,
                                StartColumn = loc.Start.Character,
                                EndLine = loc.End.Line,
                                EndColumn = loc.End.Character
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
                                StartLine = loc.Start.Line,
                                StartColumn = loc.Start.Character,
                                EndLine = loc.End.Line,
                                EndColumn = loc.End.Character
                            },
                            Message = x.GetMessage()
                        };
                    }),
                    AssemblyBytes = compilationResult.Success && !generateFiles ? stream.ToArray() : null,
                    AssemblyPath = generateFiles ? filename + ".dll" : null
                };
            }
            catch (Exception exn)
            {
                return new CompilerResult
                {
                    Error = exn
                };
            }
        }
    }
}
