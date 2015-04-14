using LinqEditor.Core.Models.Database;
using System;

namespace LinqEditor.Core.Templates
{
    public class TemplateService : ITemplateService
    {
        public string GenerateSchema(Guid schemaId, DatabaseSchema sqlSchema)
        {
            var gen = new Templates.DatabaseSchema
            {
                NamespaceId = schemaId,
                Tables = sqlSchema.Tables
            };

            return gen.TransformText();

        }

        public string GenerateQuery(Guid queryId, string sourceFragment, string schemaNamespace)
        {
            var gen = new Templates.DatabaseQuery
            {
                GeneratedSchemaNamespace = schemaNamespace,
                SourceCode = sourceFragment,
                NamespaceId = queryId
            };

            return gen.TransformText();
        }

        public string GenerateCodeStatements(Guid codeId, string sourceFragment)
        {
            var gen = new Templates.CodeStatements
            {
                SourceCode = sourceFragment,
                NamespaceId = codeId
            };

            return gen.TransformText();
        }
    }
}
