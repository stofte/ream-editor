using LinqEditor.Core.CodeAnalysis.Compiler;
using LinqEditor.Core.Helpers;
using LinqEditor.Core.Templates;
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
            var result1 = CSharpCompiler.CompileToFile(_source1, _id1.ToIdentifierWithPrefix("d"), PathUtility.TempPath);
            var result2 = CSharpCompiler.CompileToBytes(_source1, _id1.ToIdentifierWithPrefix("d"));
            _path1 = result1.AssemblyPath;
            _bytes1 = result2.AssemblyBytes;
        }

        [TestFixtureTearDown]
        public void Cleanup()
        {
            if (File.Exists(_path1))
            {

            }
        }
    }
}
