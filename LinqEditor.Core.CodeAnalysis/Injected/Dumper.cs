using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.CodeAnalysis.Injected
{
    /// <summary>
    /// Dumper is used in the generated code for showing results in the UI. 
    /// SQL column data is injected by the generated code before invocation.
    /// Each instance is in a seperate AppDomain.
    /// </summary>
    public static class Dumper
    {
        private class TypeMap
        {
            public IEnumerable<Column> Columns;
        }

        private enum ColumnType
        {
            Field,
            Property
        }

        private class Column
        {
            public int Index;
            public string Name;
            public Type Type;
            public ColumnType Kind;
        }

        private static List<DataTable> _dumps = new List<DataTable>();

        public static IDictionary<string, IDictionary<string, int>> SqlColumns { get; set; }

        public static IEnumerable<DataTable> FlushDumps()
        {
            var tbs = _dumps.AsEnumerable();
            _dumps = new List<DataTable>();
            return tbs;
        }

        public static T Dump<T>(this T o)
        {
            if (_dumps == null)
            {
                _dumps = new List<DataTable>();
            }

            if (o != null)
            {
                var table = new DataTable();
                Type objectType = o.GetType();

                TypeMap objectMap = null;

                if (IsSingular(objectType)) // string, int, etc
                {
                    table.Columns.Add(new DataColumn("Value", objectType));
                    table.Rows.Add(o);
                }
                else // will have more then one column
                {
                    if (!(o is IEnumerable)) // single object 
                    {
                        objectMap = MapType(objectType, SqlColumns);
                        table.Columns.AddRange(MapColumns(objectMap));
                        table.Rows.Add(MapRow(objectMap, o));
                    }
                    else // list of objects
                    {
                        var list = o as IEnumerable;

                        foreach (var item in list)
                        {
                            if (objectMap == null)
                            {
                                objectType = item.GetType();
                                objectMap = MapType(objectType, SqlColumns);
                                table.Columns.AddRange(MapColumns(objectMap));
                            }

                            table.Rows.Add(MapRow(objectMap, item));
                        }
                    }
                }

                _dumps.Add(table);
            }

            return o;
        }

        private static bool IsSingular(Type type)
        {
            return type == typeof(string) || type == typeof(Guid) || type == typeof(int) ||
                type == typeof(long) || type == typeof(DateTime);
        }

        private static DataColumn[] MapColumns(TypeMap map)
        {
            return map.Columns.OrderBy(x => x.Index).Select(y => new DataColumn(y.Name, y.Type)).ToArray();
        }

        private static object[] MapRow(TypeMap map, object obj)
        {
            var objType = obj.GetType();
            var rowValues = new object[map.Columns.Count()];
            foreach (var col in map.Columns)
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

        private static TypeMap MapType(Type type, IDictionary<string, IDictionary<string, int>> sqlColumns)
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

            return new TypeMap
            {
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
                }))
            };
        }
    }
}
