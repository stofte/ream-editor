using LinqEditor.Schema.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Generator.Interfaces
{
    public interface ISchemaGenerator : IGenerator
    {
        string GeneratedSchemaNamespace { get; }
    }
}
