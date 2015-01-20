using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Backend.Compiler
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
                MetadataReference.CreateFromAssembly(typeof(LinqEditor.Backend.Compiler.CompilerResult).Assembly) // LinqEditor.Backend.dll
            };
        }

        private string AssemblyFilename()
        {
            var guid = Guid.NewGuid().ToString().Replace("-", "");
            return guid;
        }

        public CompilerResult Compile(string src, string assemblyName, params string[] otherReferences) {

            try
            {
                var references = otherReferences.Length > 0 ? References.Concat(otherReferences.Select(x => MetadataReference.CreateFromFile(x))) : References;
                var compilerOptions = new CSharpCompilationOptions(outputKind: OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Debug);
                var compilation = CSharpCompilation.Create(assemblyName, new[] { CSharpSyntaxTree.ParseText(src) }, references, compilerOptions);

                // generate dll, assemblies generated in memory by roslyn,
                // does not seem loadable for queries trying to load schema dependency
                var filename = AppDomain.CurrentDomain.BaseDirectory + "gendlls\\" + assemblyName;

                EmitResult compilationResult;
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

                var stream = new MemoryStream();
                compilation.Emit(stream);

                return new CompilerResult
                {
                    //CompiledAssembly = compilationResult.Success ? Assembly.LoadFile(filename + ".dll") : null,
                    CompilationErrors = compilationResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray(),
                    CompiledAssemblyImage = compilationResult.Success ? stream.ToArray() : null,
                    CompiledAssemblyPath = filename + ".dll"
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