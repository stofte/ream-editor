using System;

namespace LinqEditor.Core.Models.Database
{
    [Serializable]
    public class ColumnSchema
    {
        public string Name;
        public Type Type;
        public int Index;
    }
}
