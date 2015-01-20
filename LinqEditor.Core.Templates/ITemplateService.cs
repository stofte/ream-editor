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
        string GenerateSchema(int schemaId, out string assemblyNamespace, IEnumerable<TableSchema> tables);
        string GenerateQuery(int queryId, out string assemblyNamespace, string sourceFragment, string schemaAssemblyNamespace);
    }
}
