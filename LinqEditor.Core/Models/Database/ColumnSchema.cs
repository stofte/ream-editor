using System;

namespace LinqEditor.Core.Models.Database
{
    [Serializable]
    public class ColumnSchema
    {
        public string Name;
        public string TypeName;
        public Type Type;
        public int Index;
    }
}
