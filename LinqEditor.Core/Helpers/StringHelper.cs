using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LinqEditor.Core.Helpers
{
    public static class StringHelper
    {
        // git normalizes text files newlinews, so this helper is used 
        // by tests to fix newlinews back to expected windows style.
        public static string NormalizeLines(this String str)
        {
            // http://stackoverflow.com/questions/1175053/regex-that-matches-a-newline-n-in-c-sharp
            var regex = new Regex(@"(\r\n?|\n)");
            return regex.Replace(str, Environment.NewLine);
        }
    }
}
