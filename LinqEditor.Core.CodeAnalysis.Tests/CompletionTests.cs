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

        [TestFixtureSetUp]
        public static void Class_Initialie(TestContext ctx)
        {
            var ns = "ns";
            var programSource = @"
using System;
using System.Linq;
using System.Collections.Generic;

namespace Generated
{
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

        [Test]
        public void Can_Initialize_Completion()
        {
            var completion = new Completion(_templateService);
            completion.Initialize();
        }

        [Test]
        public void Can_Suggest_Completions_For_MemberAccessExpression_Of_Generic_Int_List()
        {
            var completion = new Completion(_templateService) as ICompletion;
            completion.Initialize();
            var x = new List<int>();
            var str = "var x = new List<int>();x.";
            completion.UpdateFragment(str);
            var s = completion.MemberAccessExpressionCompletions(str.Length-1).Suggestions.ToArray();
            var vsEntries = new Tuple<string,SuggestionType>[]
            {
                // list created from VS2012
                T.Create("Add", SuggestionType.Method),
                T.Create("AddRange", SuggestionType.Method),
                T.Create("Aggregate", SuggestionType.ExtensionMethod),
                T.Create("All", SuggestionType.ExtensionMethod),
                T.Create("Any", SuggestionType.ExtensionMethod),
                T.Create("AsEnumerable", SuggestionType.ExtensionMethod),
                T.Create("AsParallel", SuggestionType.ExtensionMethod),
                T.Create("AsQueryable", SuggestionType.ExtensionMethod),
                T.Create("AsReadOnly", SuggestionType.Method),
                T.Create("Average", SuggestionType.ExtensionMethod),
                T.Create("BinarySearch", SuggestionType.Method),
                T.Create("Capacity", SuggestionType.Property),
                T.Create("Cast", SuggestionType.ExtensionMethod),
                T.Create("Clear", SuggestionType.Method),
                T.Create("Concat", SuggestionType.ExtensionMethod),
                T.Create("Contains", SuggestionType.Method),
                T.Create("Contains", SuggestionType.ExtensionMethod),
                T.Create("ConvertAll", SuggestionType.Method),
                T.Create("CopyTo", SuggestionType.Method),
                T.Create("Count", SuggestionType.Property),
                T.Create("Count", SuggestionType.ExtensionMethod),
                T.Create("DefaultIfEmpty", SuggestionType.ExtensionMethod),
                T.Create("Distinct", SuggestionType.ExtensionMethod),
                T.Create("ElementAt", SuggestionType.ExtensionMethod),
                T.Create("ElementAtOrDefault", SuggestionType.ExtensionMethod),
                T.Create("Equals", SuggestionType.Method),
                T.Create("Except", SuggestionType.ExtensionMethod),
                T.Create("Exists", SuggestionType.Method),
                T.Create("Find", SuggestionType.Method),
                T.Create("FindAll", SuggestionType.Method),
                T.Create("FindIndex", SuggestionType.Method),
                T.Create("FindLast", SuggestionType.Method),
                T.Create("FindLastIndex", SuggestionType.Method),
                T.Create("First", SuggestionType.ExtensionMethod),
                T.Create("FirstOrDefault", SuggestionType.ExtensionMethod),
                T.Create("ForEach", SuggestionType.Method),
                T.Create("GetEnumerator", SuggestionType.Method),
                T.Create("GetHashCode", SuggestionType.Method),
                T.Create("GetRange", SuggestionType.Method),
                T.Create("GetType", SuggestionType.Method),
                T.Create("GroupBy", SuggestionType.ExtensionMethod),
                T.Create("GroupJoin", SuggestionType.ExtensionMethod),
                T.Create("IndexOf", SuggestionType.Method),
                T.Create("Insert", SuggestionType.Method),
                T.Create("InsertRange", SuggestionType.Method),
                T.Create("Intersect", SuggestionType.ExtensionMethod),
                T.Create("Join", SuggestionType.ExtensionMethod),
                T.Create("Last", SuggestionType.ExtensionMethod),
                T.Create("LastIndexOf", SuggestionType.Method),
                T.Create("LastOrDefault", SuggestionType.ExtensionMethod),
                T.Create("LongCount", SuggestionType.ExtensionMethod),
                T.Create("Max", SuggestionType.ExtensionMethod),
                T.Create("Min", SuggestionType.ExtensionMethod),
                T.Create("OfType", SuggestionType.ExtensionMethod),
                T.Create("OrderBy", SuggestionType.ExtensionMethod),
                T.Create("OrderByDescending", SuggestionType.ExtensionMethod),
                T.Create("Remove", SuggestionType.Method),
                T.Create("RemoveAll", SuggestionType.Method),
                T.Create("RemoveAt", SuggestionType.Method),
                T.Create("RemoveRange", SuggestionType.Method),
                T.Create("Reverse", SuggestionType.Method),
                T.Create("Reverse", SuggestionType.ExtensionMethod),
                T.Create("Select", SuggestionType.ExtensionMethod),
                T.Create("SelectMany", SuggestionType.ExtensionMethod),
                T.Create("SequenceEqual", SuggestionType.ExtensionMethod),
                T.Create("Single", SuggestionType.ExtensionMethod),
                T.Create("SingleOrDefault", SuggestionType.ExtensionMethod),
                T.Create("Skip", SuggestionType.ExtensionMethod),
                T.Create("SkipWhile", SuggestionType.ExtensionMethod),
                T.Create("Sort", SuggestionType.Method),
                T.Create("Sum", SuggestionType.ExtensionMethod),
                T.Create("Take", SuggestionType.ExtensionMethod),
                T.Create("TakeWhile", SuggestionType.ExtensionMethod),
                T.Create("ToArray", SuggestionType.Method),
                T.Create("ToArray", SuggestionType.ExtensionMethod),
                T.Create("ToDictionary", SuggestionType.ExtensionMethod),
                T.Create("ToList", SuggestionType.ExtensionMethod),
                T.Create("ToLookup", SuggestionType.ExtensionMethod),
                T.Create("ToString", SuggestionType.Method),
                T.Create("TrimExcess", SuggestionType.Method),
                T.Create("TrueForAll", SuggestionType.Method),
                T.Create("Union", SuggestionType.ExtensionMethod),
                T.Create("Where", SuggestionType.ExtensionMethod),
                T.Create("Zip", SuggestionType.ExtensionMethod),
            };

            var index = 34;
            var testE = vsEntries[index];
            var compE = T.Create(s[index].Value, s[index].Kind);

            for (var i = 0; i < vsEntries.Length; i++)
            {
                Assert.IsTrue(s[i].Value == vsEntries[i].Item1 && s[i].Kind == vsEntries[i].Item2);
            }

        }
    }
}
