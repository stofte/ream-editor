using LinqEditor.Core.Schema.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.Templates
{
    public interface ITemplateService
    {
        string GenerateSchema(Guid schemaId, out string assemblyNamespace, DatabaseSchema sqlSchema);
        string GenerateQuery(Guid queryId, out string assemblyNamespace, string sourceFragment, string schemaAssemblyNamespace);
    }
}
