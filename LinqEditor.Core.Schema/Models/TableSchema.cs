using System;
using System.Collections.Generic;

namespace LinqEditor.Core.Schema.Models
{
    [Serializable]
    public class TableSchema
    {
        public string Catalog { get; set; }
        public string Schema { get; set; }
        public string Name { get; set; }

        public IEnumerable<ColumnSchema> Columns { get; set; }
    }
}
