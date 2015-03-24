using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Test.Common
{
    public static class VSDocumentationTestData
    {
        #region documentation data
        public const string Int32Doc = "int32";
        public const string HashSetDoc = "hashset`1";
        public const string DataColumnDoc = "datacolumn";
        public const string IQueryableDoc = "iqueryable`1";
        public const string IntListDoc = "intlist";
        public const string IntListCtorDoc = "intlistctor";

        public static Dictionary<string, Tuple<string, string, IEnumerable<string>, string>> DocumentationData =
            new Dictionary<string, Tuple<string, string, IEnumerable<string>, string>> 
        {
            {Int32Doc, Tuple.Create(
                "struct System.Int32", 
                "Represents a 32-bit signed integer.", 
                new List<string>{ }.AsEnumerable(),
                @"T:System.Int32")},
            {HashSetDoc, Tuple.Create(
                "class System.Collections.Generic.HashSet<T>", 
                "Represents a set of values.", 
                new List<string>{ "T is System.Int32" }.AsEnumerable(),
                @"T:System.Collections.Generic.HashSet`1")},
            {DataColumnDoc, Tuple.Create(
                "class System.Data.DataColumn", 
                "Represents the schema of a column in a System.Data.DataTable.", 
                new List<string>{  }.AsEnumerable(),
                "T:System.Data.DataColumn")},
            {IQueryableDoc, Tuple.Create(
                "interface System.Linq.IQueryable<out T>", 
                "Provides functionality to evaluate queries against a specific data source wherein the type of the data is known.", 
                new List<string>{ "T is System.Int32" }.AsEnumerable(),
                "T:System.Linq.IQueryable`1")},
            {IntListDoc, Tuple.Create(
                "class System.Collections.Generic.List<T>", 
                "Represents a strongly typed list of objects that can be accessed by index. Provides methods to search, sort, and manipulate lists.", 
                new List<string>{ "T is System.Int32" }.AsEnumerable(),
                @"T:System.Collections.Generic.List`1")},
            {IntListCtorDoc, Tuple.Create(
                "List<int>.List() (+ 2 overload(s))", 
                "Initializes a new instance of the System.Collections.Generic.List<T> class that is empty and has the default initial capacity.", 
                new List<string>{ }.AsEnumerable(),
                @"M:System.Collections.Generic.List`1.#ctor")},
        };
        #endregion

        public const string VarDeclOfIntAtZero = "VarDeclOfInt0";
        public const string VarDeclOfIntHashSetAtZero = "VarDeclOfIntHashSet0";
        public const string VarDeclOfQueryableAtZero = "VarDeclOfQueryable0";
        public const string FullDeclOfDataColumnAtZero = "FullDeclOfDataColumn0";
        public const string FullDeclOfMultipleIntsAtZero = "FullDeclOfMultipleInts0";
        public const string FullDeclerationOfIntAtZero = "FullDeclOfInt0";
        public const string VarDeclOfIntListAtZero = "VarDeclOfIntList0";

        public const string VarDeclOfQueryableAtTen = "VarDeclOfQueryable10";
        public const string VarDeclOfListWithCopyFromCtorAt56 = "VarDeclOfListWithCopyFromCtor56";

        static void Foo()
        {
            
        }

        public static Dictionary<string, Tuple<string, int, Tuple<string, string, IEnumerable<string>, string>>> Data = 
            new Dictionary<string, Tuple<string, int, Tuple<string, string, IEnumerable<string>, string>>>
        {
            // Item1 = source stub
            // Item2 = offset
            // Item3 = TypeAndName
            // Item4 = Description
            // Item5 = Specializations
            // Item6 = DocumentationId

            // access the var/decl type of the statement (index = 0)
            {VarDeclOfIntAtZero, Tuple.Create(SourceCodeFragments.VarDeclOfInt, 0, DocumentationData[Int32Doc])}, // mscorlib.dll
            {VarDeclOfIntHashSetAtZero, Tuple.Create(SourceCodeFragments.VarDeclOfIntHashSet, 0, DocumentationData[HashSetDoc])}, // System.Core.dll 4.0
            {FullDeclOfDataColumnAtZero, Tuple.Create(SourceCodeFragments.FullDeclOfDataColumn, 0, DocumentationData[DataColumnDoc])}, // System.Data.dll
            {VarDeclOfQueryableAtZero, Tuple.Create(SourceCodeFragments.VarDeclOfQueryable, 0, DocumentationData[IQueryableDoc])},
            {FullDeclOfMultipleIntsAtZero, Tuple.Create(SourceCodeFragments.FullDeclOfMultipleInts, 0, DocumentationData[Int32Doc])},
            {FullDeclerationOfIntAtZero, Tuple.Create(SourceCodeFragments.FullDeclOfInt, 0, DocumentationData[Int32Doc])},
            {VarDeclOfIntListAtZero, Tuple.Create(SourceCodeFragments.VarDeclOfIntList, 0, DocumentationData[IntListDoc])},

            // access the new construtor statement
            {VarDeclOfQueryableAtTen, Tuple.Create(SourceCodeFragments.VarDeclOfQueryable, 10, DocumentationData[IntListCtorDoc])},
            {VarDeclOfListWithCopyFromCtorAt56, Tuple.Create(SourceCodeFragments.VarDeclOfListWithCopyFromCtor, 56, DocumentationData[IntListCtorDoc])},
        };

    }
}
