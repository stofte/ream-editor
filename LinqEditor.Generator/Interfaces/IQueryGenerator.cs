using LinqEditor.Schema.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Generator.Interfaces
{
    public interface IQueryGenerator : IGenerator
    {
        string SourceCode { get; set; }
        string GeneratedSchemaNamespace { get; set; }
        string GeneratedQueryNamespace { get; }
    }
}
