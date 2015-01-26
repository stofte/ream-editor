using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.CodeAnalysis.Models
{
    public class ValidationResults
    {
        public IEnumerable<Error> Errors { get; set; }
        public IEnumerable<Warning> Warnings { get; set; }
    }
}
