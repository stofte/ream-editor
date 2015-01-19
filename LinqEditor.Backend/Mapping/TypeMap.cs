using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Backend.Mapping
{
    public class TypeMap
    {
        public IEnumerable<Column> Columns { get; private set; }

        public static bool IsSingular(Type type)
        {
            return type == typeof(string) || type == typeof(Guid) || type == typeof(int) ||
                type == typeof(long) || type == typeof(DateTime);
        }

        public object[] MapRow(object obj)
        {
            var objType = obj.GetType();
            var rowValues = new object[Columns.Count()];
            foreach (var col in Columns)
            {
                if (col.Kind == ColumnType.Field)
                {
                    rowValues[col.Index] = objType.GetField(col.Name).GetValue(obj);
                }
                else
                {
                    rowValues[col.Index] = objType.GetProperty(col.Name).GetValue(obj, null);
                }
            }
            return rowValues;
        }

        public DataColumn[] GetDataColumns()
        {
            return Columns.OrderBy(x => x.Index).Select(y => new DataColumn(y.Name, y.Type)).ToArray();
        }

        public TypeMap(Type type, IDictionary<string, IDictionary<string, int>> sqlColumns)
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

            Columns = props.Select(p => new Column
            {
                Kind = ColumnType.Property,
                Index = getIndex(typeName, p.Name),
                Name = p.Name,
                Type = p.PropertyType
            }).Concat(fields.Select(f => new Column
            {
                Kind = ColumnType.Field,
                Index = getIndex(typeName, f.Name),
                Name = f.Name,
                Type = f.FieldType
            }));
        }
    }
}
