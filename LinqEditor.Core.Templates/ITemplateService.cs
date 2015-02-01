using LinqEditor.Core.Schema.Models;
using System;

namespace LinqEditor.Core.Templates
{
    public interface ITemplateService
    {
        string GenerateSchema(Guid schemaId, DatabaseSchema sqlSchema);
        string GenerateQuery(Guid queryId, string sourceFragment, string schemaAssemblyNamespace);
        string GenerateCodeStatements(Guid codeId, string sourceFragment);
    }
}
