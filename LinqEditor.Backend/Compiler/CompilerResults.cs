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
        public byte[] AssemblyBytes { get; set; }
        public string AssemblyPath { get; set; }
        public Diagnostic[] Errors { get; set; }
        public Diagnostic[] Warnings { get; set; }
    }
}
