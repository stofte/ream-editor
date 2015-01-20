using LinqEditor.Core.Schema.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.Schema.Services
{
    public interface ISqlSchemaProvider
    {
        IEnumerable<TableSchema> GetSchema(string connectionString);
    }
}
