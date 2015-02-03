using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.CodeAnalysis.Models
{
    public class AnalysisResult
    {
        public IEnumerable<SuggestionEntry> MemberCompletions { get; set; }
        public EditContext Context { get; set; }
    }
}
