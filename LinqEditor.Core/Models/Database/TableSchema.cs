using System;
using System.Collections.Generic;

namespace LinqEditor.Core.Models.Database
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
