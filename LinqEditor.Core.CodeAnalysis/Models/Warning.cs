using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.CodeAnalysis.Models
{
    public class Warning
    {
        public string Message { get; set; }
        public LocationSpan Location { get; set; }
    }
}
