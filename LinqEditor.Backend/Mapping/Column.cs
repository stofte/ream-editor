using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Backend.Mapping
{
    public class Column
    {
        public int Index;
        public string Name;
        public Type Type;
        public ColumnType Kind;
    }
}
