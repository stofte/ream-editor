using System;

namespace LinqEditor.Core.Schema.Models
{
    [Serializable]
    public class ColumnSchema
    {
        public string Name;
        public string Type;
        public int Index;
    }
}
