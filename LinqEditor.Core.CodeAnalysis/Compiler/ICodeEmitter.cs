using Microsoft.CodeAnalysis;
using System.IO;
using System.Threading;

namespace LinqEditor.Core.CodeAnalysis.Compiler
{
    public interface ICodeEmitter
    {
        CompilerResult EmitLibrary(string source, string assemblyName, Stream dllOutput, Stream pdbOutput, MetadataReference[] references, CancellationToken ct);
    }
}
