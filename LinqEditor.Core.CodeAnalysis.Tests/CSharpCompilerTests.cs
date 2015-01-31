using LinqEditor.Core.CodeAnalysis.Compiler;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.CodeAnalysis.Tests
{
    [TestFixture]
    public class CSharpCompilerTests
    {
        [Test]
        public void Can_Compile_CSharp_Library()
        {
            var source = @"
using System;

namespace Test {
    public class TestClass {
        public int Foo { get; set; }
    }
}
";
            var compiler = new CSharpCompiler();
            var result = compiler.Compile(source, "Test");
            var assembly = Assembly.Load(result.AssemblyBytes);
            Assert.AreEqual(1, assembly.DefinedTypes.Count());
        }

        [Test]
        public void Can_Compile_CSharp_Library_To_File_On_Disk()
        {
            var source = @"
using System;

namespace Test {
    public class TestClass {
        public int Foo { get; set; }
    }
}
";
            var compiler = new CSharpCompiler();
            var result = compiler.Compile(source, "Test", Path.GetTempPath());
            var filename = Path.GetTempPath() + "Test.dll";
            Assert.IsTrue(File.Exists(filename));
        }

        [Test]
        public void Reports_Errors() 
        {
            // lacks statement terminator
            var source = @"
using System;

namespace Test {
    public class TestClass {
        public int Query() { 
            return 42 
        }
    }
}
";
            var compiler = new CSharpCompiler();
            var result = compiler.Compile(source, "Test");
            Assert.GreaterOrEqual(result.Errors.Count(), 1);
        }
    }
}
