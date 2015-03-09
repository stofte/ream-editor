using LinqEditor.Core.Models.Analysis;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Test.Common
{
    public static class ToolTipTestData
    {
        public const string VarDeclOfInt_InitialDecl = "VarDeclOfInt0"; // initial decl
        public const string VarDeclOfIntHashSet_InitialDecl = "VarDeclOfIntHashSet0"; // initial decl
        public const string VarDeclOfIntHashSet_Ctor = "VarDeclOfIntHashSet11"; // ctor
        public const string VarDeclOfIntHashSet_IntGenericTypeParam = "VarDeclOfIntHashSet24"; // int type param
        public const string VarDeclOfListWithCopyFromCtor_Ctor = "VarDeclOfListWithCopyFromCtor52"; // ctor
        public const string FullDeclOfDataColumn_InitialDecl = "FullDeclOfDataColumn0"; // initial decl
        public const string FullDeclOfDataColumn_Ctor = "FullDeclOfDataColumn19"; // ctor
        public const string VarDeclOfDictionaryWithTuples_InitialDecl = "VarDeclOfDictionaryWithTuples0"; // initial decl
        public const string VarDeclOfDictionaryWithTuples_TupleGenericTypeParam = "VarDeclOfDictionaryWithTuples37"; // tuple type param

        // key ->
        // tuple(
        //     item1: src,
        //     item2: offset,
        //     item3: tooltip
        // )
        public static Dictionary<string, Tuple<string, int, ToolTipData>> Data =
            new Dictionary<string, Tuple<string, int, ToolTipData>>
        {
            {VarDeclOfInt_InitialDecl, Tuple.Create(SourceCodeFragments.VarDeclOfInt, 0, new ToolTipData {
                ItemName = "struct System.Int32",
                Description = "Represents a 32-bit signed integer.",
                Addendums = new List<string> { }
            })}, // mscorlib.dll
            {VarDeclOfIntHashSet_InitialDecl, Tuple.Create(SourceCodeFragments.VarDeclOfIntHashSet, 0, new ToolTipData {
                ItemName = "class System.Collections.Generic.HashSet<T>",
                Description = "Represents a set of values.",
                Addendums = new List<string> { "T is System.Int32" }
            })},
            {VarDeclOfIntHashSet_Ctor, Tuple.Create(SourceCodeFragments.VarDeclOfIntHashSet, 11, new ToolTipData {
                ItemName = "HashSet<int>.HashSet() (+ 3 overload(s))",
                Description = "Initializes a new instance of the System.Collections.Generic.HashSet<T> class that is empty and uses the default equality comparer for the set type.",
                Addendums = new List<string> { }
            })},
            {VarDeclOfIntHashSet_IntGenericTypeParam, Tuple.Create(SourceCodeFragments.VarDeclOfIntHashSet, 24, new ToolTipData {
                ItemName = "struct System.Int32",
                Description = "Represents a 32-bit signed integer.",
                Addendums = new List<string> { }
            })},
            {VarDeclOfListWithCopyFromCtor_Ctor, Tuple.Create(SourceCodeFragments.VarDeclOfListWithCopyFromCtor, 52, new ToolTipData {
                ItemName = "List<int>.List(IEnumerable<int> collection) (+ 2 overload(s))",
                Description = "Initializes a new instance of the System.Collections.Generic.List<T> class that contains elements copied from the specified collection and has sufficient capacity to accommodate the number of elements copied.",
                Addendums = new List<string> { "Exceptions:", "\tSystem.ArgumentNullException" }
            })},
            {FullDeclOfDataColumn_InitialDecl, Tuple.Create(SourceCodeFragments.FullDeclOfDataColumn, 0, new ToolTipData {
                ItemName = "class System.Data.DataColumn",
                Description = "Represents the schema of a column in a System.Data.DataTable.",
                Addendums = new List<string> { }
            })},
            {FullDeclOfDataColumn_Ctor, Tuple.Create(SourceCodeFragments.FullDeclOfDataColumn, 19, new ToolTipData {
                ItemName = "DataColumn.DataColumn() (+ 4 overload(s))",
                Description = "Initializes a new instance of a System.Data.DataColumn class as type string.",
                Addendums = new List<string> { }
            })},
            {VarDeclOfDictionaryWithTuples_InitialDecl, Tuple.Create(SourceCodeFragments.VarDeclOfDictionaryWithTuples, 0, new ToolTipData {
                ItemName = "class System.Collections.Generic.Dictionary<TKey,TValue>",
                Description = "Represents a collection of keys and values.",
                Addendums = new List<string> { "TKey is System.String", "TValue is System.Tuple<System.String,System.Collections.Generic.IEnumerable<System.String>>" }
            })},
            {VarDeclOfDictionaryWithTuples_TupleGenericTypeParam, Tuple.Create(SourceCodeFragments.VarDeclOfDictionaryWithTuples, 37, new ToolTipData {
                ItemName = "class System.Tuple<T1,T2>",
                Description = "Represents a 2-tuple, or pair.",
                Addendums = new List<string> { "T1 is System.String", "T2 is System.Collections.Generic.IEnumerable<System.String>" }
            })},
        };

        static void stub()
        {
            var dict = new Dictionary<string, Tuple<string, IEnumerable<string>>>();
        }
    }
}
