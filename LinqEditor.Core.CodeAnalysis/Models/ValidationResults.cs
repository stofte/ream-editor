using System.Collections.Generic;

namespace LinqEditor.Core.CodeAnalysis.Models
{
    public class ValidationResults
    {
        public IEnumerable<Error> Errors { get; set; }
        public IEnumerable<Warning> Warnings { get; set; }
    }
}
