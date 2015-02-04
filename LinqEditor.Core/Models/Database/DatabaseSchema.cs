using System;
using System.Collections.Generic;

namespace LinqEditor.Core.Models.Database
{
    [Serializable]
    public class DatabaseSchema
    {
        public IEnumerable<TableSchema> Tables;
    }
}
