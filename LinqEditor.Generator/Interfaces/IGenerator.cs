using LinqEditor.Schema.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Generator.Interfaces
{
    public interface IGenerator
    {
        int NamespaceId { get; set; }
        IEnumerable<TableSchema> Tables { get; set; }
        string TransformText();
    }
}
