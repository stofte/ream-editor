using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.CodeAnalysis.Models
{
    public class LocationSpan
    {
        public int StartLine { get; set; }
        public int StartColumn { get; set; }

        public int EndLine { get; set; }
        public int EndColumn { get; set; }
    }
}
