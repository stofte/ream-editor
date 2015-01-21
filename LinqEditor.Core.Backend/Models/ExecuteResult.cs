using LinqEditor.Core.CodeAnalysis.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.Backend.Models
{
    // this should probably be cleaned up a bit ...
    [Serializable]
    public class ExecuteResult
    {
        public IEnumerable<DataTable> Tables { get; set; }
        public IEnumerable<Warning> Warnings { get; set; }
        public Exception Exception { get; set; }
        public Exception InternalException { get; set; }
        public bool Success { get; set; }
    }
}
