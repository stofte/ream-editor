using LinqEditor.Core.Schema.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.Backend.Settings
{
    public interface IUserSettings
    {
        string GetCachedAssembly(string connectionString);
        void PersistSchema(string connectionString, DatabaseSchema schema, string assemblyPath);
    }
}
