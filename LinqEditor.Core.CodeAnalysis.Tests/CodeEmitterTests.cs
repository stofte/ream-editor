using LinqEditor.Core.CodeAnalysis.Compiler;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace LinqEditor.Core.CodeAnalysis.Tests
{
    [TestFixture]
    public class CodeEmitterTests
    {
        MetadataReference[] _stdRefs;
        string _source = @"
public class TestClass {
    public int Foo { get; set; }
}
";

        [TestFixtureSetUp]
        public void Setup()
        {
            _stdRefs = CompilerReferences.GetStandardReferences();
        }

        [Test]
        public void EmitLibrarys_DllOutput_Is_Mandatory()
        {
            var emitter = new CodeEmitter();

            Assert.Throws<ArgumentNullException>(() =>
                emitter.EmitLibrary(source: _source, assemblyName: "test", references: _stdRefs));
        }

        [Test]
        public void EmitLibrarys_References_Is_Mandatory()
        {
            var emitter = new CodeEmitter();

            Assert.Throws<ArgumentNullException>(() =>
                emitter.EmitLibrary(source: _source, assemblyName: "test"));
        }

        [Test]
        public void EmitLibrarys_References_Cannot_Be_Empty()
        {
            var emitter = new CodeEmitter();
            var stream = new MemoryStream();

            Assert.Throws<ArgumentException>(() =>
                emitter.EmitLibrary(source: _source, dllOutput: stream, assemblyName: "test", references: new MetadataReference[] { }));
        }

        [TestCase(default(string))]
        [TestCase("")]
        [TestCase(" ")]
        public void EmitLibrarys_Source_Is_Mandatory(string src)
        {
            var emitter = new CodeEmitter();
            var stream = new MemoryStream();
            Assert.Throws<ArgumentNullException>(() =>
                emitter.EmitLibrary(source: src, assemblyName: "test", dllOutput: stream, references: _stdRefs));
        }



        [TestCase(default(string))]
        [TestCase("")]
        [TestCase(" ")]
        public void EmitLibrarys_AssemblyName_Is_Mandatory(string src)
        {
            var emitter = new CodeEmitter();
            var stream = new MemoryStream();
            Assert.Throws<ArgumentNullException>(() =>
                emitter.EmitLibrary(source: _source, dllOutput: stream, references: _stdRefs));
        }

        [Test]
        public void EmitLibrary_Outputs_To_Dll_Stream()
        {
            var emitter = new CodeEmitter();
            var stream = new MemoryStream();

            Assert.AreEqual(0, stream.Length);
            var res = emitter.EmitLibrary(source: _source, assemblyName: "test", dllOutput: stream, references: _stdRefs);
            Assert.Greater(stream.Length, 100);
        }

        [Test]
        public void EmiteLibrary_Resets_Stream_Position()
        {
            var emitter = new CodeEmitter();
            var stream = new MemoryStream();
            var pdbStream = new MemoryStream();

            Assert.AreEqual(0, stream.Position);
            Assert.AreEqual(0, pdbStream.Position);
            var res = emitter.EmitLibrary(_source, "test", stream, pdbStream, _stdRefs, CancellationToken.None);
            Assert.AreEqual(0, stream.Position);
            Assert.AreEqual(0, pdbStream.Position);
        }

        [Test]
        public void EmiteLibrary_Allows_Null_For_PdbStream()
        {
            var emitter = new CodeEmitter();
            var stream = new MemoryStream();

            var res = emitter.EmitLibrary(_source, "test", stream, null, _stdRefs, CancellationToken.None);
        }

        [Test]
        public void Can_Load_Emitted_Library()
        {
            var emitter = new CodeEmitter();
            var stream = new MemoryStream();
            var pdbStream = new MemoryStream();

            Assert.AreEqual(0, stream.Length);
            Assert.AreEqual(0, pdbStream.Length);
            var res = emitter.EmitLibrary(_source, "test", stream, pdbStream, _stdRefs, CancellationToken.None);
            var bytes = new byte[stream.Length];
            stream.Read(bytes, 0, (int)stream.Length);
            var assm = Assembly.Load(bytes);

            Assert.AreEqual(1, assm.DefinedTypes.Count());
        }

        [Test]
        public void EmitLibrary_Returns_Errors()
        {
            var emitter = new CodeEmitter();
            var stream = new MemoryStream();
            var res = emitter.EmitLibrary(source: "not valid source", assemblyName: "test", dllOutput: stream, references: _stdRefs);

            Assert.GreaterOrEqual(res.Errors.Count(), 1);
        }

        [Test]
        public void EmitLibrary_Returns_Warnings()
        {
            var source = @"
namespace Test {
    public class TestClass {
        public void Foo() {
            var notReferenced = 0x2a;
        };
    }
}
";
            var emitter = new CodeEmitter();
            var stream = new MemoryStream();
            var res = emitter.EmitLibrary(source: source, assemblyName: "test", dllOutput: stream, references: _stdRefs);

            Assert.GreaterOrEqual(res.Warnings.Count(), 1);
        }
    }
}
