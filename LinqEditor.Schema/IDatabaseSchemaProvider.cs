using LinqEditor.Core.Models.Database;
using LinqEditor.Core.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Schema
{
    public interface IDatabaseSchemaProvider
    {
        Task<DatabaseSchema> GetDatabaseSchema(Connection connection);
    }
}
