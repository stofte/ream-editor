using LinqEditor.Core.CodeAnalysis.Compiler;
using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Reflection;

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

            var result = CSharpCompiler.CompileToBytes(source, "Test");
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

            var result = CSharpCompiler.CompileToFile(source, "Test", Path.GetTempPath());
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

            var result = CSharpCompiler.CompileToBytes(source, "Test");
            Assert.GreaterOrEqual(result.Errors.Count(), 1);
        }
    }
}
