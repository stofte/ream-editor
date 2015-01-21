using LinqEditor.Core.CodeAnalysis.Models;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.CodeAnalysis.Compiler
{
    public class CompilerResult
    {
        public bool Success { get; set; }
        public Exception Error { get; set; }
        public byte[] AssemblyBytes { get; set; }
        public string AssemblyPath { get; set; }
        public IEnumerable<Error>  Errors { get; set; }
        //public Diagnostic[] Warnings { get; set; }

        public IEnumerable<Warning> Warnings { get; set; }
    }
}
