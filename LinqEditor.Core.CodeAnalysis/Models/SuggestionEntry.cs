using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.CodeAnalysis.Models
{
    public class SuggestionEntry
    {
        public SuggestionType Kind { get; set; }
        public string Value { get; set; }
    }
}
