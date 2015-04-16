using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.Models.Database
{
    [Serializable]
    public class ServerSchema
    {
        public IEnumerable<DatabaseSchema> Databases { get; set; }
    }
}
