using LinqEditor.Core.Models.Database;
using LinqEditor.Core.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.Schema.Services
{
    public class SQLiteSchemaProvider : ISchemaProvider
    {
        public DatabaseSchema GetSchema(Connection connection)
        {
            return null;
        }
    }
}
