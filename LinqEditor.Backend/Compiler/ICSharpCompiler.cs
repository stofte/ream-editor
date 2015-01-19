using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Backend.Compiler
{
    public interface ICSharpCompiler
    {
        CompilerResult Compile(string src, string assemblyName, params string[] references);
    }
}
