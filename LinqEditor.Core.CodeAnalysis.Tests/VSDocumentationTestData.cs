using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.CodeAnalysis.Tests
{
    public static class VSDocumentationTestData
    {
        public const string VarDeclerationOfInt32 = "Int32";
        public const string VarDeclerationOfHashSet = "HashSet";
        public const string VarDeclerationOfDataColumn = "DataColumn";
        public const string VarDeclerationOfQueryable = "ListAsQueryable";
        public const string VarDeclerationOfMultipleInts = "MultipleInts";

        static void Foo()
        {
            int x1 = 10, x2 = 20, x3 = 42;
        }

        public static Dictionary<string, Tuple<string, int, string, string, IEnumerable<string>, string>> Data = 
            new Dictionary<string, Tuple<string, int, string, string, IEnumerable<string>, string>>
        {
            // Item1 = source stub
            // Item2 = offset
            // Item3 = TypeAndName
            // Item4 = Description
            // Item5 = Specializations
            // Item6 = DocumentationId
            {VarDeclerationOfInt32, Tuple.Create(// mscorlib.dll
                @"var x = 0x2a;", 0, 
                "struct System.Int32", 
                "Represents a 32-bit signed integer.", 
                new List<string>{ }.AsEnumerable(),
                @"T:System.Int32")},
            {VarDeclerationOfHashSet, Tuple.Create(// System.Core.dll 4.0
                @"var set = new HashSet<int>();", 0, 
                "class System.Collections.Generic.HashSet<T>", 
                "Represents a set of values", 
                new List<string>{ "T is System.Int32" }.AsEnumerable(),
                @"T:System.Collections.Generic.HashSet`1")},
            {VarDeclerationOfDataColumn, Tuple.Create( // System.Data.dll
                @"DataColumn col = new DataColumn();", 0, 
                "class System.Data.DataColumn", 
                "Represents the schema of a column in a System.Data.DataTable.", 
                new List<string>{  }.AsEnumerable(),
                "T:System.Data.DataColumn")},
            {VarDeclerationOfQueryable, Tuple.Create(
                @"var x = new List<int>().AsQueryable();", 0, 
                "interface System.Linq.IQueryable<out T>", 
                "Provides functionality to evaluate queries against a specific data source wherein the type of the data is known.", 
                new List<string>{  }.AsEnumerable(),
                "T:System.Linq.IQueryable`1")},
            {VarDeclerationOfMultipleInts, Tuple.Create(
                @"int x1 = 10, x2 = 20, x3 = 42;", 0, 
                "struct System.Int32", 
                "Represents a 32-bit signed integer.", 
                new List<string>{ }.AsEnumerable(),
                @"T:System.Int32")},
        };
    }
}
