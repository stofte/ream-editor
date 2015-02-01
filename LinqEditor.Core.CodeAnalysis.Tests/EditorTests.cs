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
        }

        [Test]
        public void Can_Initialize_Editor()
        {
            var editor = new Services.Editor(_templateService);
            editor.Initialize();
        }
    }
}
