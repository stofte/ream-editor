using System;
using System.Collections.Generic;

namespace LinqEditor.Core.Models.Database
{
    [Serializable]
    public class DatabaseSchema
    {
        public string ConnectionString { get; set; }
        public IEnumerable<TableSchema> Tables { get; set; }
    }
}
