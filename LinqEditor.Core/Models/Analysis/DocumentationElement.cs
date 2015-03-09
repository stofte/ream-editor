using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.Models.Analysis
{
    public class DocumentationElement
    {
        public string Id { get; set; }
        public string Summary { get; set; }
        // names of parameters as per docs + description
        public IEnumerable<Tuple<string, string>> MethodParameters { get; set; }
        // any exceptions declared by the docs
        public IEnumerable<string> MethodExceptions { get; set; }
    }
}
