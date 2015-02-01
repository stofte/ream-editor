using System;
using System.Collections.Generic;

namespace LinqEditor.Core.Schema.Models
{
    [Serializable]
    public class DatabaseSchema
    {
        public IEnumerable<TableSchema> Tables;
    }
}
