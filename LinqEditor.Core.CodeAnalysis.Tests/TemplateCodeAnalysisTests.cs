using LinqEditor.Core.CodeAnalysis.Services;
using LinqEditor.Core.Models.Analysis;
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
            m.Setup(s => s.GenerateCodeStatements(It.IsAny<Guid>(), It.IsAny<string>())).Returns(programSource);
            _templateService = m.Object;
        }

        // "." is last char
        [TestCase("var x = new List<int>(); x.", 1, UserContext.MemberCompletion, Description = "generic integer list")]
        [TestCase("var x = new List<int>(); x.Select(x => x.", 1, UserContext.MemberCompletion, Description = "lambda argument")]
        [TestCase("int.", 1, UserContext.MemberCompletion, Description = "type alias")]
        [TestCase("MyStatic.", 1, UserContext.MemberCompletion, Description = "static class")]
        [TestCase("var s = new MyStruct { Bar = 22 }; s.", 1, UserContext.MemberCompletion, Description = "instance struct")]
        [TestCase("MyStruct.", 1, UserContext.MemberCompletion, Description = "static struct")]
        [TestCase("this.", 1, UserContext.MemberCompletion, Description = "this")]
        // not member completions
        [TestCase("var x = new List<int>();", 1, UserContext.Unknown, Description = "statement terminator")]
        [TestCase("3423", 1, UserContext.Unknown, Description = "illegal token?")]
        public void Returns_Correct_Context_For_Analyze(string src, int offset, UserContext editContex)
        {
            
            var editor = new TemplateCodeAnalysis(_templateService);
            editor.Initialize();
            //_context.UpdateContext("", ""); // fire bogus event to init class
            var result = editor.Analyze(src, src.Length - offset);
            Assert.AreEqual(editContex, result.Context);
        }

        [TestCase("MyStatic.", "MyStaticClass", 1, Description = "static custom class")]
        [TestCase("var s = new MyStruct { Bar = 22 }; s.", "MyStructInstance", 1, Description = "instance custom struct")]
        [TestCase("MyStruct.", "MyStructStatic", 1, Description = "static access")]
        [TestCase("int.", "IntegerAlias", 1, Description = "int type alias")]
        [TestCase("var x = new List<int>();x.", "GenericIntegerListInstance", 1, Description = "generic int list")]
        [TestCase("var x = new List<int>();x.Where(y => y.", "IntegerValueInstance", 1, Description = "int instance 1")]
        [TestCase("var x = new List<int>();x.Where(y => y.)", "IntegerValueInstance", 2, Description = "int instance 2, add closing paren")]
        [TestCase("var x = new List<int>();x.Where(y => new { y.})", "IntegerValueInstance", 3, Description = "int instance 2, add closing paren")]
        public void Returns_Member_Access_Completions(string src, string vsEntriesKey, int offset)
        {
            var editor = new TemplateCodeAnalysis(_templateService);
            editor.Initialize();
            var vsEntries = VSCompletionTestData.Data[vsEntriesKey];
            var result = editor.Analyze(src, src.Length - offset);
            var suggesitons = result.MemberCompletions.ToArray();

            var debugstr1 = string.Join("|", suggesitons.Select(x => x.Value).OrderBy(x => x));
            var debugstr2 = string.Join("|", vsEntries.Select(x => x.Item1).OrderBy(x => x));

            for (var i = 0; i < vsEntries.Length; i++)
            {
                Assert.AreEqual(suggesitons[i].Value, vsEntries[i].Item1);
                Assert.AreEqual(suggesitons[i].Kind, vsEntries[i].Item2);
            }
        }

        //[Test]
        public void Can_Detect_LinqEditor_Core_Generated_Dumper_Extension_Methods()
        {
            var programSource = @"
using System;
using System.Linq;
using System.Collections.Generic;
using LinqEditor.Core.Generated;

namespace Test
{
    public class Program
    {
        public void Query() 
        {
" + SchemaConstants.Marker + @"
        }
    }
}
";

            var m = new Mock<ITemplateService>();
            m.Setup(s => s.GenerateQuery(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).Returns(programSource);
            m.Setup(s => s.GenerateCodeStatements(It.IsAny<Guid>(), It.IsAny<string>())).Returns(programSource);
            
            var editor = new TemplateCodeAnalysis(m.Object);
            editor.Initialize();

            var stub = "var x = new List<int>();x.";

            var result = editor.Analyze(stub, stub.Length - 1);

            var dumpMethod = result.MemberCompletions.Where(x => x.Value == "Dump" && x.Kind == MemberKind.ExtensionMethod);

            Assert.AreSame(dumpMethod.Count(), 1);
        }
    }
}
