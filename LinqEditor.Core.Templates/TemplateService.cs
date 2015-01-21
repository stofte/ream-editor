using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.Templates
{
    public class TemplateService : ITemplateService
    {
        public string GenerateSchema(Guid schemaId, out string schemaNamespace, IEnumerable<LinqEditor.Core.Schema.Models.TableSchema> tables)
        {
            var gen = new Schema.SqlServer()
            {
                NamespaceId = schemaId,
                Tables = tables
            };

            schemaNamespace = gen.GeneratedSchemaNamespace;

            return gen.TransformText();

        }

        public string GenerateQuery(Guid queryId, out string assemblyNamespace, string sourceFragment, string schemaNamespace)
        {
            var gen = new Query.SqlServer()
            {
                GeneratedSchemaNamespace = schemaNamespace,
                SourceCode = sourceFragment,
                NamespaceId = queryId
            };

            assemblyNamespace = gen.GeneratedQueryNamespace;

            return gen.TransformText();
        }
    }
}
