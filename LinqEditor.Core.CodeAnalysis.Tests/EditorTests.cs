using LinqEditor.Core.CodeAnalysis.Models;
using LinqEditor.Core.Context;
using LinqEditor.Core.Templates;
using Moq;
using NUnit.Framework;
using System;

namespace LinqEditor.Core.CodeAnalysis.Tests
{
    [TestFixture]
    public class EditorTests
    {
        private ITemplateService _templateService;
        private IContext _context; 

        [TestFixtureSetUp]
        public void Initialize()
        {
            var programSource = @"
using System;
using System.Linq;
using System.Collections.Generic;

namespace First.Generated {
    public static class MyStatic { public static int MyValue = 42; }
    public struct MyStruct 
    { 
        public static uint Foo = 0xdeadbeef; 
        public int Bar { get; set; } 
        public int Baz() { return 42; }
    }
}

namespace Another.Generated
{
    using First.Generated;

    public abstract class ProgramBase { protected int MyInheritedProperty { get; set; } }
    public class Program
    {
        protected int MyProperty { get; set; }
        public void Query() 
        {
" + SchemaConstants.Marker + @"
        }
    }
}
";
            var m = new Mock<ITemplateService>();
            m.Setup(s => s.GenerateQuery(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).Returns(programSource);
            _templateService = m.Object;
            _context = new Context.Context();
        }

        [TestCase("var x = new List<int>(); x.", Description="list")]
        [TestCase("var x = new List<int>(); x.Select(x => x.", Description="lambda argument")]
        [TestCase("int.", Description = "type alias")]
        [TestCase("MyStatic.", Description = "static class")]
        [TestCase("var s = new MyStruct { Bar = 22 }; s.", Description = "instance struct")]
        [TestCase("MyStruct.", Description = "static struct")]
        [TestCase("this.", Description="this")]
        public void Editor_Returns_MemberCompletion_Context_For_Update(string src)
        {
            
            var editor = new Services.Editor(_templateService, _context);
            _context.UpdateContext("", ""); // fire bogus event
            var ctx = editor.UpdateSource(src, src.Length - 1);
            Assert.AreEqual(EditContext.MemberCompletion, ctx);
        }


    }
}
