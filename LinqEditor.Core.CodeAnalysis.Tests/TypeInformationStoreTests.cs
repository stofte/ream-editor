using LinqEditor.Core.CodeAnalysis.Compiler;
using LinqEditor.Core.CodeAnalysis.Repositories;
using LinqEditor.Core.Context;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.CodeAnalysis.Tests
{
    [TestFixture]
    public class TypeInformationStoreTests
    {
        IContext _context;
        string _assemblyPath;
        string _source1 = @"
using System;
using System.Linq;
using System.Collections.Generic;

namespace TypeTest 
{
    public class Program
    {
        public string Execute() 
        {
            
        }
    }
}";

        [TestFixtureSetUp]
        public void Initialize()
        {
            var result = CSharpCompiler.CompileToFile(_source1, "TypeTest", PathUtility.TempPath);
            _assemblyPath = result.AssemblyPath;
            _context = new Context.Context();
        }

        [TestFixtureTearDown]
        public void Cleanup()
        {
            var file = PathUtility.TempPath + "TypeTest.dll";
            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }

        [Test]
        public void Can_Construct_TypeInformationStore_And_Update_Context()
        {
            var store = new TypeInformationStore(_context);
            _context.UpdateContext(_assemblyPath, "TypeTest");
        }
    }
}
