using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.Backend.Models
{
    /// <summary>
    /// Wraps the result of initializing the linked appdomain
    /// </summary>
    [Serializable]
    public class InitializeResult
    {
        public Exception Exception { get; set; }
        public string AssemblyPath { get; set; }
        public string SchemaNamespace { get; set; }
    }
}
