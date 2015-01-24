using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.Schema.Models
{
    [Serializable]
    public class DatabaseSchema
    {
        public IEnumerable<TableSchema> Tables;
    }
}
