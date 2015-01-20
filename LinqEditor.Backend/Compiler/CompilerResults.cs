using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Backend.Compiler
{
    public class CompilerResult
    {
        public Exception Error { get; set; }
        public Assembly CompiledAssembly { get; set; }
        public byte[] CompiledAssemblyImage { get; set; }
        public string CompiledAssemblyPath { get; set; }
        public Diagnostic[] CompilationErrors { get; set; }
    }
}
