using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.Schema.Models
{
    public class TableSchema
    {
        public string Catalog { get; set; }
        public string Schema { get; set; }
        public string Name { get; set; }

        public IEnumerable<ColumnSchema> Columns { get; set; }
    }
}
