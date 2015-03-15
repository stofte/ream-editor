using LinqEditor.Core.Models.Analysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Test.Common
{
    public static class ToolTipTestData
    {


        // key ->
        // tuple(
        //     item1: src,
        //     item2: offset,
        //     item3: tooltip
        // )
        
        // VS doesn't include a space in the type param/arg list, but since the ToDisplayString method in roslyn does, it's easier to align with roslyn.
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
            {VarDeclOfListWithCopyFromCtor_Ctor, Tuple.Create(SourceCodeFragments.VarDeclOfListWithCopyFromCtor, 53, new ToolTipData {
                ItemName = "List<int>.List(IEnumerable<int> collection) (+ 2 overload(s))",
                Description = "Initializes a new instance of the System.Collections.Generic.List<T> class that contains elements copied from the specified collection and has sufficient capacity to accommodate the number of elements copied.",
                Addendums = new List<string> { "Exceptions:", "\tSystem.ArgumentNullException" }
            })},
            {FullDeclOfDataColumn_InitialDecl, Tuple.Create(SourceCodeFragments.FullDeclOfDataColumn, 3, new ToolTipData {
                ItemName = "class System.Data.DataColumn",
                Description = "Represents the schema of a column in a System.Data.DataTable.",
                Addendums = new List<string> { }
            })},
            {FullDeclOfDataColumn_Ctor, Tuple.Create(SourceCodeFragments.FullDeclOfDataColumn, 20, new ToolTipData {
                ItemName = "DataColumn.DataColumn() (+ 4 overload(s))",
                Description = "Initializes a new instance of a System.Data.DataColumn class as type string.",
                Addendums = new List<string> { }
            })},
            {VarDeclOfDictionaryWithTuples_InitialDecl, Tuple.Create(SourceCodeFragments.VarDeclOfDictionaryWithTuples, 0, new ToolTipData {
                ItemName = "class System.Collections.Generic.Dictionary<TKey, TValue>",
                Description = "Represents a collection of keys and values.",
                // TODO: figure out why there's no space here!
                Addendums = new List<string> { "TKey is System.String", "TValue is System.Tuple<System.String, System.Collections.Generic.IEnumerable<System.String>>" }
            })},
            {VarDeclOfDictionaryWithTuples_TupleGenericTypeParam, Tuple.Create(SourceCodeFragments.VarDeclOfDictionaryWithTuples, 37, new ToolTipData {
                ItemName = "class System.Tuple<T1, T2>",
                Description = "Represents a 2-tuple, or pair.",
                Addendums = new List<string> { "T1 is System.String", "T2 is System.Collections.Generic.IEnumerable<System.String>" }
            })},
            {FullDeclOfDataColumn_MemberProperty, Tuple.Create(SourceCodeFragments.FullDeclOfDataColumn, 55, new ToolTipData {
                ItemName = "object DataColumn.DefaultValue",
                Description = "Gets or sets the default value for the column when you are creating new rows.",
                Addendums = new List<string> { "Exceptions:", "\tSystem.InvalidCastException" }
            })},
            {ComplexStatementsExampleOne_TupleItem2Property, Tuple.Create(SourceCodeFragments.ComplexStatementsExampleOne, 340, new ToolTipData {
                ItemName = "int Tuple<int, int>.Item2",
                Description = "Gets the value of the current System.Tuple<T1, T2> object's second component.",
                Addendums = new List<string> { }
            })},
            {ComplexStatementsExampleTwo_TupleItem2Property, Tuple.Create(SourceCodeFragments.ComplexStatementsExampleTwo, 660, new ToolTipData {
                ItemName = "IEnumerable<int> Tuple<int, IEnumerable<int>>.Item2",
                Description = "Gets the value of the current System.Tuple<T1, T2> object's second component.",
                Addendums = new List<string> { }
            })},
            {ComplexStatementsExampleTwo_SecondTupleItem2Property, Tuple.Create(SourceCodeFragments.ComplexStatementsExampleTwo, 535, new ToolTipData {
                ItemName = "IEnumerable<int> Tuple<int, IEnumerable<int>>.Item2",
                Description = "Gets the value of the current System.Tuple<T1, T2> object's second component.",
                Addendums = new List<string> { }
            })},
            {ComplexStatementsExampleTwo_StringTypeReference, Tuple.Create(SourceCodeFragments.ComplexStatementsExampleTwo, 612, new ToolTipData {
                ItemName = "class System.String",
                Description = "Represents text as a series of Unicode characters.",
                Addendums = new List<string> { }
            })},
            {ComplexStatementsExampleTwo_StringJoinMethod, Tuple.Create(SourceCodeFragments.ComplexStatementsExampleTwo, 618, new ToolTipData {
                ItemName = "string string.Join<int>(string separator, IEnumerable<int> values) (+ 4 overload(s))",
                Description = "Concatenates the members of a collection, using the specified separator between each member.",
                Addendums = new List<string> { "Exceptions:", "\tSystem.ArgumentNullException" }
            })},
            {ComplexStatementsExampleTwo_SumExtensionMethod, Tuple.Create(SourceCodeFragments.ComplexStatementsExampleTwo, 540, new ToolTipData {
                ItemName = "(extension) int IEnumerable<int>.Sum() (+ 10 overload(s))",
                Description = "Computes the sum of a sequence of System.Int32 values.",
                Addendums = new List<string> { "Exceptions:", "\tSystem.ArgumentNullException", "\tSystem.OverflowException" }
            })},
            {ComplexStatementsExampleTwo_SelectExtensionMethod, Tuple.Create(SourceCodeFragments.ComplexStatementsExampleTwo, 598, new ToolTipData {
                ItemName = "(extension) IQueryable<string> IQueryable<Tuple<int, IEnumerable<int>>>.Select<Tuple<int, IEnumerable<int>>, string>(Expression<Func<Tuple<int, IEnumerable<int>>, int, string>> selector) (+ 3 overload(s))",
                Description = "Projects each element of a sequence into a new form by incorporating the element's index.",
                Addendums = new List<string> { "Exceptions:", "\tSystem.ArgumentNullException" }
            })},
            {ComplexStatementsExampleTwo_OtherSelectExtensionMethod, Tuple.Create(SourceCodeFragments.ComplexStatementsExampleTwo, 798, new ToolTipData {
                ItemName = "(extension) IEnumerable<int> IEnumerable<Tuple<int, IEnumerable<int>>>.Select<Tuple<int, IEnumerable<int>>, int>(Func<Tuple<int, IEnumerable<int>>, int> selector) (+ 3 overload(s))",
                Description = "Projects each element of a sequence into a new form.",
                Addendums = new List<string> { "Exceptions:", "\tSystem.ArgumentNullException" }
            })},
            {ComplexStatementsExampleTwo_ConcatExtensionMethod, Tuple.Create(SourceCodeFragments.ComplexStatementsExampleTwo, 651, new ToolTipData {
                ItemName = "(extension) IEnumerable<int> IEnumerable<int>.Concat<int>(IEnumerable<int> second)",
                Description = "Concatenates two sequences.",
                Addendums = new List<string> { "Exceptions:", "\tSystem.ArgumentNullException" }
            })},
            {ComplexStatementsExampleTwo_FirstZParameterReference, Tuple.Create(SourceCodeFragments.ComplexStatementsExampleTwo, 579, new ToolTipData {
                ItemName = "(parameter) Tuple<int, IEnumerable<int>> z",
                Description = "",
                Addendums = new List<string> { }
            })},
            {ComplexStatementsExampleTwo_SecondZParameterReference, Tuple.Create(SourceCodeFragments.ComplexStatementsExampleTwo, 591, new ToolTipData {
                ItemName = "(parameter) Tuple<int, IEnumerable<int>> z",
                Description = "",
                Addendums = new List<string> { }
            })},
            {ComplexStatementsExampleTwo_SelectIndexParameterReference, Tuple.Create(SourceCodeFragments.ComplexStatementsExampleTwo, 605, new ToolTipData {
                ItemName = "(parameter) int i",
                Description = "",
                Addendums = new List<string> { }
            })},
            {ComplexStatementsExampleTwo_MyVarLocalVariableReference, Tuple.Create(SourceCodeFragments.ComplexStatementsExampleTwo, 913, new ToolTipData {
                ItemName = "(local variable) int myVar",
                Description = "",
                Addendums = new List<string> { }
            })},
        };

        static void stub()
        {
var from = new List<Tuple<int, IEnumerable<int>>> 
{
    Tuple.Create(1, new List<int> { 2, 4, 10 }.AsEnumerable()), 
    Tuple.Create(3, new List<int> { 2, 4, 16 }.AsEnumerable()), 
    Tuple.Create(5, new List<int> { 6, 6, 14 }.AsEnumerable()),
    Tuple.Create(7, new List<int> { 4, 8, 18 }.AsEnumerable()), 
    Tuple.Create(9, new List<int> { 2, 6, 20 }.AsEnumerable())
};

var x = new List<Tuple<int, IEnumerable<int>>>(from).AsQueryable();
Func<Tuple<int, IEnumerable<int>>, bool> filter = (t) =>
{
    return t.Item2.Sum() < 20;
};
var filtered = x.Where(z => filter(z)).Select((z,i) => string.Join("", "", new int[]{ z.Item1 }.Concat(z.Item2)));
Func<Tuple<int, IEnumerable<int>>, int> selectFunc = t =>
{
    return t.Item1;
};
var filtered2 = x.Where(z => filter(z)).Select(selectFunc);
var myVar = x.Max(z => z.Item1); var myComplexVar = x;
var filtered3 = x.Max(z => z.Item1 + myVar + myComplexVar.Max(y => y.Item1));
        }

        public const string VarDeclOfInt_InitialDecl = "VarDeclOfInt_InitialDecl"; // initial decl
        public const string VarDeclOfIntHashSet_InitialDecl = "VarDeclOfIntHashSet_InitialDecl"; // initial decl
        public const string VarDeclOfIntHashSet_Ctor = "VarDeclOfIntHashSet_Ctor"; // ctor
        public const string VarDeclOfIntHashSet_IntGenericTypeParam = "VarDeclOfIntHashSet_IntGenericTypeParam"; // int type param
        public const string VarDeclOfListWithCopyFromCtor_Ctor = "VarDeclOfListWithCopyFromCtor_Ctor"; // ctor
        public const string FullDeclOfDataColumn_InitialDecl = "FullDeclOfDataColumn_InitialDecl"; // initial decl
        public const string FullDeclOfDataColumn_Ctor = "FullDeclOfDataColumn_Ctor"; // ctor
        public const string FullDeclOfDataColumn_MemberProperty = "FullDeclOfDataColumn_MemberProperty"; // property access
        public const string VarDeclOfDictionaryWithTuples_InitialDecl = "VarDeclOfDictionaryWithTuples_InitialDecl"; // initial decl
        public const string VarDeclOfDictionaryWithTuples_TupleGenericTypeParam = "VarDeclOfDictionaryWithTuples_TupleGenericTypeParam"; // tuple type param

        public const string ComplexStatementsExampleOne_TupleItem2Property = "ComplexStatementsExampleOne_TupleItem2Property";
        public const string ComplexStatementsExampleTwo_TupleItem2Property = "ComplexStatementsExampleTwo_TupleItem2Property";
        public const string ComplexStatementsExampleTwo_SecondTupleItem2Property = "ComplexStatementsExampleTwo_SecondTupleItem2Property";
        public const string ComplexStatementsExampleTwo_StringTypeReference = "ComplexStatementsExampleTwo_StringTypeReference";
        public const string ComplexStatementsExampleTwo_StringJoinMethod = "ComplexStatementsExampleTwo_StringJoinMethod";
        public const string ComplexStatementsExampleTwo_SumExtensionMethod = "ComplexStatementsExampleTwo_SumExtensionMethod";
        public const string ComplexStatementsExampleTwo_SelectExtensionMethod = "ComplexStatementsExampleTwo_SelectExtensionMethod";
        public const string ComplexStatementsExampleTwo_ConcatExtensionMethod = "ComplexStatementsExampleTwo_ConcatExtensionMethod";
        public const string ComplexStatementsExampleTwo_OtherSelectExtensionMethod = "ComplexStatementsExampleTwo_OtherSelectExtensionMethod";
        public const string ComplexStatementsExampleTwo_FirstZParameterReference = "ComplexStatementsExampleTwo_FirstZParameterReference";
        public const string ComplexStatementsExampleTwo_SecondZParameterReference = "ComplexStatementsExampleTwo_SecondZParameterReference";
        public const string ComplexStatementsExampleTwo_SelectIndexParameterReference = "ComplexStatementsExampleTwo_SelectIndexParameterReference";
        public const string ComplexStatementsExampleTwo_MyVarLocalVariableReference = "ComplexStatementsExampleTwo_MyVarLocalVariableReference";
    }
}
