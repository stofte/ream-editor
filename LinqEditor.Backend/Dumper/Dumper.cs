using LinqEditor.Backend.Mapping;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Backend.Dumper
{
    public static class Dumper
    {
        public static DumperResult Result { get; set; }

        public static T Dump<T>(this T o)
        {
            if (o != null)
            {
                var table = new DataTable();
                Type objectType = o.GetType();
                
                TypeMap objectMap = null;

                if (TypeMap.IsSingular(objectType)) // string, int, etc
                {
                    table.Columns.Add(new DataColumn("Value", objectType));
                    table.Rows.Add(o);
                }
                else // will have more then one column
                {
                    if (!(o is IEnumerable)) // single object 
                    {
                        objectMap = new TypeMap(objectType, Result.SqlColumns);
                        table.Columns.AddRange(objectMap.GetDataColumns());
                        table.Rows.Add(objectMap.MapRow(o));
                    }
                    else // list of objects
                    {
                        var list = o as IEnumerable;

                        foreach (var item in list)
                        {
                            if (objectMap == null)
                            {
                                objectType = item.GetType();
                                objectMap = new TypeMap(objectType, Result.SqlColumns);
                                table.Columns.AddRange(objectMap.GetDataColumns());
                            }

                            table.Rows.Add(objectMap.MapRow(item));
                        }
                    }
                }

                Result.AddTable(table);
            }

            return o;
        }
    }
}