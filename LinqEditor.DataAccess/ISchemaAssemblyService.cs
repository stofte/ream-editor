using LinqEditor.Core.Models.Database;
using LinqEditor.Core.Models.Editor;
using LinqEditor.Core.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.DataAccess
{
    public interface ISchemaAssemblyProvider
    {
        Task<DatabaseSchema> Load(Connection connection);
    }
}
