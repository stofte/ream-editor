using LinqEditor.Core.CodeAnalysis.Compiler;
using LinqEditor.Core.Helpers;
using LinqEditor.Core.Templates;
using LinqEditor.Core.Containers;
using NUnit.Framework;
using System;
using System.IO;

namespace LinqEditor.Core.Tests
{
    [TestFixture]
    public class CodeContainerTests
    {
        Guid _id1;
        string _source1;
        string _path1;
        byte[] _bytes1;
        
        [TestFixtureSetUp]
        public void Initialize()
        {
            var templateService = new TemplateService();
            _id1 = Guid.NewGuid();
            _source1 = templateService.GenerateCodeStatements(_id1, "Write(\"foobar\");");
            var result1 = CSharpCompiler.CompileToFile(_source1, _id1.ToIdentifierWithPrefix(SchemaConstants.CodePrefix), PathUtility.TempPath);
            var result2 = CSharpCompiler.CompileToBytes(_source1, _id1.ToIdentifierWithPrefix(SchemaConstants.CodePrefix));
            _path1 = result1.AssemblyPath;
            _bytes1 = result2.AssemblyBytes;
        }

        [TestFixtureTearDown]
        public void Cleanup()
        {
            if (File.Exists(_path1))
            {
                File.Delete(_path1);
            }
        }

        [Test]
        public void Can_Execute_Simple_Statements_With_Write()
        {
            var container = new Isolated<CodeContainer>(Guid.NewGuid());
            container.Value.Initialize();
            var result = container.Value.Execute(_path1);
            container.Dispose();
            Assert.AreEqual("foobar", result.CodeOutput);
        }
    }
}
