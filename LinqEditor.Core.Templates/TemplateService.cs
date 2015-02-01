using LinqEditor.Core.Schema.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.Templates
{
    public class TemplateService : ITemplateService
    {
        public string GenerateSchema(Guid schemaId, DatabaseSchema sqlSchema)
        {
            var gen = new Schema.SqlServer()
            {
                NamespaceId = schemaId,
                Tables = sqlSchema.Tables
            };

            return gen.TransformText();

        }

        public string GenerateQuery(Guid queryId, string sourceFragment, string schemaNamespace)
        {
            var gen = new Query.SqlServer()
            {
                GeneratedSchemaNamespace = schemaNamespace,
                SourceCode = sourceFragment,
                NamespaceId = queryId
            };

            return gen.TransformText();
        }

        public string GenerateCodeStatements(Guid codeId, string sourceFragment)
        {
            var gen = new Code.Statements()
            {
                SourceCode = sourceFragment,
                NamespaceId = codeId
            };

            return gen.TransformText();
        }
    }
}
