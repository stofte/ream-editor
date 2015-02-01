using System.Collections.Generic;

namespace LinqEditor.Core.CodeAnalysis.Models
{
    public class SuggestionList
    {
        public IEnumerable<SuggestionEntry> Suggestions { get; set; }
        public bool Applicable { get; set; }
    }
}
