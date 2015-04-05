using LinqEditor.Core.Settings;
using System;
using System.Collections.Generic;

namespace LinqEditor.Core.Models.Database
{
    [Serializable]
    public class DatabaseSchema
    {
        public string AssemblyPath { get; set; }
        public Connection Connection { get; set; }
        public IEnumerable<TableSchema> Tables { get; set; }
    }
}
