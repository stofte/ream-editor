﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace LinqEditor.Core.Generated
{
    /// <summary>
    /// Dumper is used in the generated code for dumping ienumerables
    /// and other values to datatables, that are then shown in the ui.
    /// </summary>
    public static class Dumper
    {
        private class TypeMap
        {
            public IEnumerable<Column> Columns;
            public bool IsSingular { get { return Columns.Count() == 1 && Columns.First().Kind == ColumnType.Object; } }
            public int TypeCount = 1; // for display purposes only
        }

        private enum ColumnType
        {
            Field,
            Property,
            Object
        }

        private class Column
        {
            public int Index;
            public string Name;
            public Type Type;
            public ColumnType Kind;
        }

        static List<DataTable> _dumps = new List<DataTable>();
        static IDictionary<string, TypeMap> _typeMaps = new Dictionary<string, TypeMap>();
        static IDictionary<string, int> _dumpCounts = new Dictionary<string, int>();
        static object _lock = new object();

        public static IDictionary<string, IDictionary<string, int>> SqlColumns { get; set; }
        


        public static IEnumerable<DataTable> FlushDumps()
        {
            var tbs = _dumps.AsEnumerable();
            _dumps = new List<DataTable>();
            _typeMaps = new Dictionary<string, TypeMap>();
            return tbs;
        }

        public static T Dump<T>(this T o)
        {
            if (_dumps == null)
            {
                _dumps = new List<DataTable>();
            }

            if (_typeMaps == null) 
            {
                _typeMaps = new Dictionary<string, TypeMap>();
            }

            if (o != null)
            {
                var table = new DataTable();
                
                Type objectType = o.GetType();
                int dumpCount = 1;

                TypeMap objectMap = null;

                if (IsSingular(objectType)) // string, int, etc
                {
                    objectMap = MapType(objectType);
                    dumpCount = objectMap.TypeCount++;
                    table.Columns.AddRange(MapColumns(objectMap));
                    table.Rows.Add(o);
                }
                else // will have more then one column
                {
                    if (!(o is IEnumerable)) // single object 
                    {
                        objectMap = MapType(objectType);
                        dumpCount = objectMap.TypeCount++;
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
                                objectMap = MapType(objectType);
                                dumpCount = objectMap.TypeCount++;
                                table.Columns.AddRange(MapColumns(objectMap));
                            }

                            if (objectMap.IsSingular)
                            {
                                table.Rows.Add(item);
                            }
                            else
                            {
                                table.Rows.Add(MapRow(objectMap, item));
                            }
                        }
                    }
                }

                var name = GetTypeDisplayName(objectType);
                table.TableName = string.Format("{0} {1}", name, dumpCount);
                _dumps.Add(table);
            }

            return o;
        }

        private static string GetTypeDisplayName(Type type)
        {
            return CheckIfAnonymousType(type) ? "Anonymous" : type.Name;
        }

        //http://stackoverflow.com/questions/2483023/how-to-test-if-a-type-is-anonymous
        private static bool CheckIfAnonymousType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            // HACK: The only way to detect anonymous types right now.
            return Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false)
                // msnet uses AnonymousType, while mono uses AnonType
                && type.IsGenericType && (type.Name.Contains("AnonymousType") || type.Name.Contains("AnonType"))
                && (type.Name.StartsWith("<>") || type.Name.StartsWith("VB$"))
                && (type.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic;
        }

        private static bool IsSingular(Type type)
        {
            return type == typeof(string) || type == typeof(Guid) || type == typeof(int) ||
                type == typeof(long) || type == typeof(DateTime);
        }

        private static DataColumn[] MapColumns(TypeMap map)
        {
            // need to handle nullable specially
            // http://stackoverflow.com/questions/6251431/dataset-does-not-support-system-nullable
            return map.Columns.OrderBy(x => x.Index).Select(y => new DataColumn(y.Name, Nullable.GetUnderlyingType(y.Type) ?? y.Type)).ToArray();
        }

        private static object[] MapRow(TypeMap map, object obj)
        {
            var objType = obj.GetType();
            var rowValues = new object[map.Columns.Count()];
            foreach (var col in map.Columns)
            {
                if (col.Kind == ColumnType.Field)
                {
                    rowValues[col.Index] = objType.GetField(col.Name).GetValue(obj) ?? DBNull.Value;
                }
                else
                {
                    rowValues[col.Index] = objType.GetProperty(col.Name).GetValue(obj, null) ?? DBNull.Value;
                }
            }
            return rowValues;
        }

        private static TypeMap MapType(Type type)
        {
            if (_typeMaps.ContainsKey(type.AssemblyQualifiedName))
            {
                return _typeMaps[type.AssemblyQualifiedName];
            }

            if (IsSingular(type))
            {
                _typeMaps[type.AssemblyQualifiedName] = new TypeMap
                {
                    Columns = new Column[] { new Column { Index = 0, Kind = ColumnType.Object, Name = "Value", Type = type } }
                };

                return _typeMaps[type.AssemblyQualifiedName];
            }

            var sqlColumns = SqlColumns;
            var typeName = type.Name;
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => x.CanRead);
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

            Func<string, string, int, int> getIndex = (string name, string column, int count) =>
            {
                // todo: might mix indeces depending on name parameter
                if (sqlColumns.ContainsKey(name) &&
                        sqlColumns[name].ContainsKey(column))
                {
                    return sqlColumns[name][column];
                }

                // the number of the underlying collection
                return count;
            };

            _typeMaps[type.AssemblyQualifiedName] = new TypeMap
            {
                Columns = props.Select((p, idx) => new Column
                {
                    Kind = ColumnType.Property,
                    Index = getIndex(typeName, p.Name, idx),
                    Name = p.Name,
                    Type = p.PropertyType
                }).Concat(fields.Select((f, idx) => new Column
                {
                    Kind = ColumnType.Field,
                    Index = getIndex(typeName, f.Name, props.Count() + idx),
                    Name = f.Name,
                    Type = f.FieldType
                }))
            };

            return _typeMaps[type.AssemblyQualifiedName];
        }
    }
}
