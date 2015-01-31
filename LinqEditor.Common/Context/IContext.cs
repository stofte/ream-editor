using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Common.Context
{
    public interface IContext
    {
        //string AssemblyPath { get; set; }
        //string SchemaNamespace { get; set; }
        void UpdateContext(string assemblyPath, string schemaNamespace);
        event Action<string, string> ContextUpdated;
    }
}
