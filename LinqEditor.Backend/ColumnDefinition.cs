using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Backend
{
    public class ColumnDefinition
    {
        public int Index;
        public string Name;
        public Type Type;
        public ColumnKind Kind;

        public static IEnumerable<ColumnDefinition> Map(Type type, IDictionary<string, IDictionary<string, int>> sqlColumns)
        {
            var typeName = type.Name;
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => x.CanRead);
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            var autoIndex = 0;

            Func<string, string, int> getIndex = (string name, string column) =>
            {
                // todo: might mix indeces depending on name parameter
                if (sqlColumns.ContainsKey(name) &&
                        sqlColumns[name].ContainsKey(column))
                {
                    return sqlColumns[name][column];
                }
                
                return autoIndex++;
            };

            return props.Select(p => new ColumnDefinition
            {
                Kind = ColumnKind.Property,
                Index = getIndex(typeName, p.Name),
                Name = p.Name,
                Type = p.PropertyType
            }).Concat(fields.Select(f => new ColumnDefinition
            {
                Kind = ColumnKind.Field,
                Index = getIndex(typeName, f.Name),
                Name = f.Name,
                Type = f.FieldType
            }));
        }
    }
}
