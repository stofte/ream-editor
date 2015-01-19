using LinqEditor.Schema.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Schema.Interface
{
    public interface ISchemaProvider
    {
        IEnumerable<TableSchema> GetSchema(string connectionString);
    }
}
