using LinqEditor.Core.Models.Analysis;
using System;
using System.Linq;
using System.Collections.Generic;
using T = System.Tuple;

namespace LinqEditor.Test.Common
{
    public static class VSCompletionTestData
    {
        public const string ThisAccessInDb = "DbContainerThisAccess";
        public const string FooColumnAccessInDb = "DbContainerFooAccess";

        public static Dictionary<string, Tuple<string, int, IEnumerable<Tuple<string, MemberKind>>>> SourceAndData =
            new Dictionary<string, Tuple<string, int, IEnumerable<Tuple<string, MemberKind>>>>
        {
            // Item1 = source stub
            // Item2 = offset
            // Item3 = completions list (string,type)
            {ThisAccessInDb, 
                Tuple.Create("this.", 4,
                    new Tuple<string, MemberKind>[] {
                        T.Create("Foo", MemberKind.Property),
                        T.Create("TypeTestTable", MemberKind.Property)
                    }.AsEnumerable())
            },
            {FooColumnAccessInDb, 
                Tuple.Create("Foo.Select(x => x.", 17,
                    new Tuple<string, MemberKind>[] {
                        T.Create("Description", MemberKind.Property),
                        T.Create("Dump", MemberKind.ExtensionMethod),
                        T.Create("Equals", MemberKind.Method),
                        T.Create("GetHashCode", MemberKind.Method),
                        T.Create("GetType", MemberKind.Method),
                        T.Create("Id", MemberKind.Property),
                        T.Create("ToString", MemberKind.Method),
                    }.AsEnumerable())
            },
        };

        // lets us pass constant keys to access the test data 
        public static Dictionary<string, Tuple<string, MemberKind>[]> Data = new Dictionary<string, Tuple<string, MemberKind>[]>
        {
            {"ContainerType", new Tuple<string, MemberKind>[] {
                T.Create("MyProperty", MemberKind.Property)
            }},

            {"MyStaticClass", new Tuple<string, MemberKind>[] {
                T.Create("Equals", MemberKind.Method),
                T.Create("MyValue", MemberKind.Field),
                T.Create("ReferenceEquals", MemberKind.Method),
            }},

            {"MyStructInstance", new Tuple<string, MemberKind>[] {
                T.Create("Bar", MemberKind.Property),
                T.Create("Baz", MemberKind.Method),
                T.Create("Equals", MemberKind.Method),
                T.Create("GetHashCode", MemberKind.Method),
                T.Create("GetType", MemberKind.Method),
                T.Create("ToString", MemberKind.Method),
            }},

            {"MyStructStatic", new Tuple<string, MemberKind>[] {
                T.Create("Equals", MemberKind.Method),
                T.Create("Foo", MemberKind.Field),
                T.Create("ReferenceEquals", MemberKind.Method),
            }},

            {"IntegerAlias", new Tuple<string, MemberKind>[] {
                T.Create("Equals", MemberKind.Method),
                T.Create("MaxValue", MemberKind.Field),
                T.Create("MinValue", MemberKind.Field),
                T.Create("Parse", MemberKind.Method),
                T.Create("ReferenceEquals", MemberKind.Method),
                T.Create("TryParse", MemberKind.Method),
            }},

            {"IntegerValueInstance", new Tuple<string, MemberKind>[] {
                T.Create("CompareTo", MemberKind.Method),
                T.Create("Equals", MemberKind.Method),
                T.Create("GetHashCode", MemberKind.Method),
                T.Create("GetType", MemberKind.Method),
                T.Create("GetTypeCode", MemberKind.Method),
                T.Create("ToString", MemberKind.Method),
            }},

            {"GenericIntegerListInstance", new Tuple<string, MemberKind>[] {
                T.Create("Add", MemberKind.Method),
                T.Create("AddRange", MemberKind.Method),
                T.Create("Aggregate", MemberKind.ExtensionMethod),
                T.Create("All", MemberKind.ExtensionMethod),
                T.Create("Any", MemberKind.ExtensionMethod),
                T.Create("AsEnumerable", MemberKind.ExtensionMethod),
                T.Create("AsParallel", MemberKind.ExtensionMethod),
                T.Create("AsQueryable", MemberKind.ExtensionMethod),
                T.Create("AsReadOnly", MemberKind.Method),
                T.Create("Average", MemberKind.ExtensionMethod),
                T.Create("BinarySearch", MemberKind.Method),
                T.Create("Capacity", MemberKind.Property),
                T.Create("Cast", MemberKind.ExtensionMethod),
                T.Create("Clear", MemberKind.Method),
                T.Create("Concat", MemberKind.ExtensionMethod),
                T.Create("Contains", MemberKind.Method),
                T.Create("Contains", MemberKind.ExtensionMethod),
                T.Create("ConvertAll", MemberKind.Method),
                T.Create("CopyTo", MemberKind.Method),
                T.Create("Count", MemberKind.Property),
                T.Create("Count", MemberKind.ExtensionMethod),
                T.Create("DefaultIfEmpty", MemberKind.ExtensionMethod),
                T.Create("Distinct", MemberKind.ExtensionMethod),
                T.Create("ElementAt", MemberKind.ExtensionMethod),
                T.Create("ElementAtOrDefault", MemberKind.ExtensionMethod),
                T.Create("Equals", MemberKind.Method),
                T.Create("Except", MemberKind.ExtensionMethod),
                T.Create("Exists", MemberKind.Method),
                T.Create("Find", MemberKind.Method),
                T.Create("FindAll", MemberKind.Method),
                T.Create("FindIndex", MemberKind.Method),
                T.Create("FindLast", MemberKind.Method),
                T.Create("FindLastIndex", MemberKind.Method),
                T.Create("First", MemberKind.ExtensionMethod),
                T.Create("FirstOrDefault", MemberKind.ExtensionMethod),
                T.Create("ForEach", MemberKind.Method),
                T.Create("GetEnumerator", MemberKind.Method),
                T.Create("GetHashCode", MemberKind.Method),
                T.Create("GetRange", MemberKind.Method),
                T.Create("GetType", MemberKind.Method),
                T.Create("GroupBy", MemberKind.ExtensionMethod),
                T.Create("GroupJoin", MemberKind.ExtensionMethod),
                T.Create("IndexOf", MemberKind.Method),
                T.Create("Insert", MemberKind.Method),
                T.Create("InsertRange", MemberKind.Method),
                T.Create("Intersect", MemberKind.ExtensionMethod),
                T.Create("Join", MemberKind.ExtensionMethod),
                T.Create("Last", MemberKind.ExtensionMethod),
                T.Create("LastIndexOf", MemberKind.Method),
                T.Create("LastOrDefault", MemberKind.ExtensionMethod),
                T.Create("LongCount", MemberKind.ExtensionMethod),
                T.Create("Max", MemberKind.ExtensionMethod),
                T.Create("Min", MemberKind.ExtensionMethod),
                T.Create("OfType", MemberKind.ExtensionMethod),
                T.Create("OrderBy", MemberKind.ExtensionMethod),
                T.Create("OrderByDescending", MemberKind.ExtensionMethod),
                T.Create("Remove", MemberKind.Method),
                T.Create("RemoveAll", MemberKind.Method),
                T.Create("RemoveAt", MemberKind.Method),
                T.Create("RemoveRange", MemberKind.Method),
                T.Create("Reverse", MemberKind.Method),
                T.Create("Reverse", MemberKind.ExtensionMethod),
                T.Create("Select", MemberKind.ExtensionMethod),
                T.Create("SelectMany", MemberKind.ExtensionMethod),
                T.Create("SequenceEqual", MemberKind.ExtensionMethod),
                T.Create("Single", MemberKind.ExtensionMethod),
                T.Create("SingleOrDefault", MemberKind.ExtensionMethod),
                T.Create("Skip", MemberKind.ExtensionMethod),
                T.Create("SkipWhile", MemberKind.ExtensionMethod),
                T.Create("Sort", MemberKind.Method),
                T.Create("Sum", MemberKind.ExtensionMethod),
                T.Create("Take", MemberKind.ExtensionMethod),
                T.Create("TakeWhile", MemberKind.ExtensionMethod),
                T.Create("ToArray", MemberKind.Method),
                T.Create("ToArray", MemberKind.ExtensionMethod),
                T.Create("ToDictionary", MemberKind.ExtensionMethod),
                T.Create("ToList", MemberKind.ExtensionMethod),
                T.Create("ToLookup", MemberKind.ExtensionMethod),
                T.Create("ToString", MemberKind.Method),
                T.Create("TrimExcess", MemberKind.Method),
                T.Create("TrueForAll", MemberKind.Method),
                T.Create("Union", MemberKind.ExtensionMethod),
                T.Create("Where", MemberKind.ExtensionMethod),
                T.Create("Zip", MemberKind.ExtensionMethod),
            }}
        };
    }
}
