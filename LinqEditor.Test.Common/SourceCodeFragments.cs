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
    }
}
