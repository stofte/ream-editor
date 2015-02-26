using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.Models.Analysis
{
    public class AnalysisResult
    {
        public AnalysisResult()
        {
            Errors = new List<Error>();
            Warnings = new List<Warning>();
            MemberCompletions = new List<CompletionEntry>();
        }

        public IEnumerable<CompletionEntry> MemberCompletions { get; set; }
        public ToolTipData ToolTip { get; set; }
        public UserContext Context { get; set; }
        public IEnumerable<Warning> Warnings { get; set; }
        public IEnumerable<Error> Errors { get; set; }
        public bool Success
        {
            get 
            {
                return Errors == null || Errors.Count() == 0;
            }
        }
    }
}
