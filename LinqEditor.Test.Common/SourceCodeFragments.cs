using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Test.Common
{
    public static class SourceCodeFragments
    {
        public const string ErrorExample1 = "v ar x = 10;";
        public const string ErrorExample2 = @"
var x = new List<int>();
x.DoesNotExists();
";

        public const string VarDeclOfInt = @"var x = 0x2a;";
        public const string FullDeclOfInt = @"int x1 = 10;";
        public const string VarDeclOfIntHashSet = @"var set = new HashSet<int>();";
        public const string VarDeclOfDictionaryWithTuples = @"var dict = new Dictionary<string, Tuple<string, IEnumerable<string>>>();";
        public const string FullDeclOfDataColumn = @"
DataColumn col = new DataColumn();
var x = col.DefaultValue;
";
        public const string VarDeclOfQueryable = @"var x = new List<int>(10).AsQueryable();";
        public const string VarDeclOfListWithCopyFromCtor = @"
var from = new List<int> { 1, 2, 3, 4 };
var x = new List<int>(from).AsQueryable();
";
        public const string FullDeclOfMultipleInts = @"int x1 = 10, x2 = 20, x3 = 42;";
        public const string VarDeclOfIntList = @"var x = new List<int>();";

        public const string ComplexStatementsExampleOne = @"
var from = new List<Tuple<int, int>> 
{
    Tuple.Create(1,2), Tuple.Create(3,4), Tuple.Create(5,6), Tuple.Create(7,8), Tuple.Create(9, 0)
};
var x = new List<Tuple<int, int>>(from).AsQueryable();
Func<Tuple<int, int>, bool> filter = (t) => 
{
    return true;
};
var filtered = x.Where(z => filter(z)).Select(z => z.Item1 + z.Item2).TakeWhile(z => z < 10);
";

        public const string ComplexStatementsExampleTwo = @"
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
var filtered = x.Where(z => filter(z)).Select(z => String.Join("", "", new int[]{ z.Item1 }.Concat(z.Item2)));
Func<Tuple<int, IEnumerable<int>>, int> selectFunc = t =>
{
    return t.Item1;
};
var filtered2 = x.Where(z => filter(z)).Select(selectFunc);
";
    }
}
