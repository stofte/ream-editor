using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.Models.Analysis
{
    /// <summary>
    /// Represents the elements that make up the info in an tooltip.
    /// </summary>
    public class ToolTipData
    {
        public string TypeAndName { get; set; }
        public string Description { get; set; }
        public IEnumerable<string> Specializations { get; set; }

        public override string ToString()
        {
            // formats similarly to VS
            return string.Format("{0}{1}{2}{3}{4}", 
                /*0*/ TypeAndName,
                /*1*/ !string.IsNullOrWhiteSpace(Description) ? "\n" : string.Empty,
                /*2*/ !string.IsNullOrWhiteSpace(Description) ? Description : string.Empty,
                /*3*/ Specializations != null && Specializations.Count() > 0 ? "\n\n" : string.Empty,
                /*4*/ Specializations != null && Specializations.Count() > 0 ? string.Join("\n", Specializations) : string.Empty
            );
        }
    }
}
