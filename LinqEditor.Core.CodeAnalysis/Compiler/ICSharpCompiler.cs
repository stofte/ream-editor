using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.CodeAnalysis.Compiler
{
    public interface ICSharpCompiler
    {
        CompilerResult Compile(string src, string assemblyName, string outputFolder = null, params string[] references);
        CompilerResult Compile(string src, string assemblyName, string outputFolder = null, byte[] reference = null);
    }
}
