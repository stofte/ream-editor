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
    }
}
