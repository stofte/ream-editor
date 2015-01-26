using System;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using LinqEditor.Core.Templates;
using Moq;
using LinqEditor.Core.CodeAnalysis.Editor;
using LinqEditor.Core.CodeAnalysis.Models;
using T = System.Tuple;

namespace LinqEditor.Core.CodeAnalysis.Tests
{
    [TestFixture]
    public class CompletionTests
    {
        private static ITemplateService _templateService;
        private ICompletion _completion;

        [TestFixtureSetUp]
        public void Initialize()
        {
            var ns = "ns";
            var programSource = @"
using System;
using System.Linq;
using System.Collections.Generic;

namespace Generated
{
    public static class MyStatic { public static int MyValue = 42; }
    public class Proram
    {
        public void Query() 
        {
" + Completion.Marker + @"
        }
    }
}
";
            var m = new Mock<ITemplateService>();
            m.Setup(s => s.GenerateQuery(It.IsAny<Guid>(), out ns, It.IsAny<string>(), It.IsAny<string>())).Returns(programSource);
            _templateService = m.Object;
        }

        [SetUp]
        public void Setup()
        {
            _completion = new Completion(_templateService) as ICompletion;
            _completion.Initialize();
        }

        private void AssertEntries(Tuple<string, SuggestionType>[] vsEntries, SuggestionEntry[] s)
        {
            for (var i = 0; i < vsEntries.Length; i++)
            {
                Assert.AreEqual(s[i].Value, vsEntries[i].Item1);
                Assert.AreEqual(s[i].Kind, vsEntries[i].Item2);
            }
        }

        [Test]
        public void Can_Suggestion_Completions_For_MemberAccessExpression_Of_Static_Class()
        {
            var str = "MyStatic.";
            _completion.UpdateFragment(str);
            var s = _completion.MemberAccessExpressionCompletions(str.Length - 1).Suggestions.ToArray();

            AssertEntries(VSCompletionTestData.MyStaticClass, s);
        }

        [Test]
        public void Can_Suggest_Completions_For_MemberAccessExpression_Of_Integer_Inside_Lambda()
        {
            var str = "var x = new List<int>();x.Where(y => y.";
            _completion.UpdateFragment(str);
            var s = _completion.MemberAccessExpressionCompletions(str.Length - 1).Suggestions.ToArray();
            var vsEntries = VSCompletionTestData.IntegerValueInstance;

            AssertEntries(VSCompletionTestData.IntegerValueInstance, s);
        }

        [Test]
        public void Can_Suggest_Completions_For_MemberAccessExpression_Of_Integer_List()
        {
            var str = "var x = new List<int>();x.";
            _completion.UpdateFragment(str);
            var s = _completion.MemberAccessExpressionCompletions(str.Length - 1).Suggestions.ToArray();

            AssertEntries(VSCompletionTestData.GenericIntegerListInstance, s);
        }
    }
}
