using LinqEditor.Core.CodeAnalysis.Services;
using LinqEditor.Core.Models.Analysis;
using LinqEditor.Core.Templates;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LinqEditor.Core.CodeAnalysis.Tests
{
    [TestFixture]
    public class TemplateCodeAnalysisTests
    {
        IDocumentationService _mockDocumentationService;
        IDocumentationService _realDocumentationService;
        ITemplateService _templateService;
        string _simpleProgram = @"
using System;
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
        string _simpleProgramWithAllUsings = @"
using System;
using System.Data;
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

            var docMock = new Mock<IDocumentationService>();
            _mockDocumentationService = docMock.Object;

            _realDocumentationService = new DocumentationService();
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

            var editor = new TemplateCodeAnalysis(_templateService, _mockDocumentationService);
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
            var editor = new TemplateCodeAnalysis(_templateService, _mockDocumentationService);
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

        [Test]
        public void Can_Detect_LinqEditor_Core_Generated_Dumper_Extension_Methods()
        {
            var m = new Mock<ITemplateService>();
            m.Setup(s => s.GenerateQuery(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).Returns(_simpleProgram);
            m.Setup(s => s.GenerateCodeStatements(It.IsAny<Guid>(), It.IsAny<string>())).Returns(_simpleProgram);

            var editor = new TemplateCodeAnalysis(m.Object, _mockDocumentationService);
            editor.Initialize();

            var stub = "var x = new List<int>();x.";

            var result = editor.Analyze(stub, stub.Length - 1);

            Assert.IsNotNull(result.MemberCompletions.FirstOrDefault(x => x.Value == "Dump" && x.Kind == MemberKind.ExtensionMethod));
        }

        // VSToolTipTestData map
        // Item1 = source stub
        // Item2 = offset
        // Item3 = TypeAndName
        // Item4 = Description
        // Item5 = Specializations
        // Item6 = DocumentationId

        
        [TestCase(VSDocumentationTestData.VarDeclerationOfInt32, 0, UserContext.ToolTip)]
        [TestCase(VSDocumentationTestData.FullDeclerationOfInt32, 0, UserContext.ToolTip)]
        [TestCase(VSDocumentationTestData.VarDeclerationOfHashSet, 0, UserContext.ToolTip)]
        [TestCase(VSDocumentationTestData.FullDeclerationOfDataColumn, 0, UserContext.ToolTip)]
        [TestCase(VSDocumentationTestData.VarDeclerationOfQueryable, 0, UserContext.ToolTip)]
        [TestCase(VSDocumentationTestData.FullDeclerationOfMultipleInts, 0, UserContext.ToolTip)]
        [TestCase(VSDocumentationTestData.VarDeclerationOfInt32, 9, UserContext.Unknown)] // => int literal
        [TestCase(VSDocumentationTestData.VarDeclerationOfHashSet, 5, UserContext.Unknown)] // => var name
        [TestCase(VSDocumentationTestData.VarDeclerationOfQueryable, 30, UserContext.Unknown)] // => method
        public void Can_Corrrect_Return_ToolTip_Context_Information_For_Variables(string testDataKey, int offset, UserContext ctx)
        {
            var m = new Mock<ITemplateService>();
            m.Setup(s => s.GenerateQuery(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).Returns(_simpleProgramWithAllUsings);
            m.Setup(s => s.GenerateCodeStatements(It.IsAny<Guid>(), It.IsAny<string>())).Returns(_simpleProgramWithAllUsings);

            var editor = new TemplateCodeAnalysis(m.Object, _realDocumentationService);
            editor.Initialize();
            var testData = VSDocumentationTestData.Data[testDataKey];

            var result = editor.Analyze(testData.Item1, testData.Item2 + offset);
            
            Assert.AreEqual(ctx, result.Context);
        }
    }
}
