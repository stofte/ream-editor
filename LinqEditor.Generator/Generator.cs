using LinqEditor.Generator.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Generator
{
    public class Generator : IGenerator
    {
        public string GenerateSchema(int schemaId, out string schemaNamespace, IEnumerable<LinqEditor.Schema.Model.TableSchema> tables)
        {
            var gen = new Schema.SqlServer()
            {
                NamespaceId = schemaId,
                Tables = tables
            };

            schemaNamespace = gen.GeneratedSchemaNamespace;
            
            return gen.TransformText();

        }

        public string GenerateQuery(int queryId, out string assemblyNamespace, string sourceFragment, string schemaNamespace)
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
