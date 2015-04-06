using LinqEditor.Core.CodeAnalysis.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.IO;
using System.Threading;

namespace LinqEditor.Core.CodeAnalysis.Compiler
{
    /// <summary>
    /// Generates bytecode from C# source code
    /// </summary>
    public class CodeEmitter : ICodeEmitter
    {
        public CompilerResult EmitLibrary(
            string source = default(string),
            string assemblyName = default(string),
            Stream dllOutput = null,
            Stream pdbOutput = null,
            MetadataReference[] references = null,
            CancellationToken cancellationToken = default(CancellationToken)
        )
        {
            if (dllOutput == null) throw new ArgumentNullException("dllOutput");
            if (string.IsNullOrWhiteSpace(source)) throw new ArgumentNullException("source");
            if (string.IsNullOrWhiteSpace(assemblyName)) throw new ArgumentNullException("assemblyName");
            if (references == null) throw new ArgumentNullException("references");
            if (references.Length == 0) throw new ArgumentException("references cannot be empty");

            var compilerOptions = new CSharpCompilationOptions(outputKind: OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Debug);
            var tree = CSharpSyntaxTree.ParseText(source);

            var compilation = CSharpCompilation.Create(assemblyName)
                .WithOptions(compilerOptions)
                .AddReferences(references)
                .AddSyntaxTrees(new SyntaxTree[] { tree });

            var emitResult = compilation.Emit(peStream: dllOutput, pdbStream: pdbOutput, cancellationToken: cancellationToken);

            dllOutput.Position = 0;
            if (pdbOutput != null)
            {
                pdbOutput.Position = 0;
            }

            return new CompilerResult
            {
                Success = emitResult.Success,
                Errors = CodeAnalysisHelper.GetErrors(emitResult.Diagnostics),
                Warnings = CodeAnalysisHelper.GetWarnings(emitResult.Diagnostics),
            };
        }
    }
}
