using LinqEditor.Core.CodeAnalysis.Models;
using LinqEditor.Core.CodeAnalysis.Services;
using LinqEditor.Core.Context;
using LinqEditor.Core.Templates;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;

namespace LinqEditor.Core.CodeAnalysis.Tests
{
    [TestFixture]
    public class TemplateCodeAnalysisTests
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

        // "." is last char
        [TestCase("var x = new List<int>(); x.", UserContext.MemberCompletion, Description = "generic integer list")]
        [TestCase("var x = new List<int>(); x.Select(x => x.", UserContext.MemberCompletion, Description = "lambda argument")]
        [TestCase("int.", UserContext.MemberCompletion, Description = "type alias")]
        [TestCase("MyStatic.", UserContext.MemberCompletion, Description = "static class")]
        [TestCase("var s = new MyStruct { Bar = 22 }; s.", UserContext.MemberCompletion, Description = "instance struct")]
        [TestCase("MyStruct.", UserContext.MemberCompletion, Description = "static struct")]
        [TestCase("this.", UserContext.MemberCompletion, Description = "this")]
        // not member completions
        [TestCase("var x = new List<int>();", UserContext.Unknown, Description = "statement terminator")]
        [TestCase("3423", UserContext.Unknown, Description = "illegal token?")]
        public void TemplateCodeAnalysis_Returns_Correct_Context_For_Analyse(string src, UserContext editContex)
        {
            
            var editor = new TemplateCodeAnalysis(_templateService, _context);
            _context.UpdateContext("", ""); // fire bogus event to init class
            var result = editor.Analyze(src, src.Length - 1);
            Assert.AreEqual(editContex, result.Context);
        }

        [TestCase("MyStatic.", "MyStaticClass", Description = "static custom class")]
        [TestCase("var s = new MyStruct { Bar = 22 }; s.", "MyStructInstance", Description = "instance custom struct")]
        [TestCase("MyStruct.", "MyStructStatic", Description = "static access")]
        [TestCase("var x = new List<int>();x.Where(y => y.", "IntegerValueInstance", Description = "int instance")]
        [TestCase("int.", "IntegerAlias", Description = "int type alias")]
        [TestCase("var x = new List<int>();x.", "GenericIntegerListInstance", Description = "generic int list")]
        public void TemplateCodeAnalysis_Returns_Member_Access_Completions(string src, string vsEntriesKey)
        {
            var editor = new TemplateCodeAnalysis(_templateService, _context);
            var vsEntries = VSCompletionTestData.Data[vsEntriesKey];
            _context.UpdateContext("", ""); // fire bogus event to init class
            var result = editor.Analyze(src, src.Length - 1);
            var suggesitons = result.MemberCompletions.ToArray();

            var debugstr1 = string.Join("|", suggesitons.Select(x => x.Value).OrderBy(x => x));
            var debugstr2 = string.Join("|", vsEntries.Select(x => x.Item1).OrderBy(x => x));

            for (var i = 0; i < vsEntries.Length; i++)
            {
                Assert.AreEqual(suggesitons[i].Value, vsEntries[i].Item1);
                Assert.AreEqual(suggesitons[i].Kind, vsEntries[i].Item2);
            }
        }
    }
}
