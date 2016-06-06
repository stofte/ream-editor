namespace QueryEngine.Services
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using QueryEngine.Models;
    using System.Text.RegularExpressions;
    // cant reuse types from project itself so doing some silly stuff here
    using DumpInternal = System.Collections.Generic.IDictionary<
        string,
        System.Tuple<
            System.Collections.Generic.IEnumerable<
                System.Tuple<
                    string,
                    string
                >
            >,
            object
        >
    >;
    using System.Collections.Generic;

    public class QueryService
    {
        CompileService _compiler;
        DatabaseContextService _databaseContextService;
        SchemaService _schemaService;
        FragmentService _fragmentService;

        public QueryService(CompileService compiler, DatabaseContextService databaseContextService, SchemaService schemaService, FragmentService fragmentService)
        {
            _compiler = compiler;
            _databaseContextService = databaseContextService;
            _schemaService = schemaService;
            _fragmentService = fragmentService;
        }

        public DumpInternal ExecuteQuery(QueryInput input)
        {
            var sw = new Stopwatch();
            sw.Start();
            var newInput = _fragmentService.Fix(input.Text);
            var contextResult = _databaseContextService.GetDatabaseContext(input.ConnectionString);
            var assmName = Guid.NewGuid().ToIdentifierWithPrefix("a");
            var programSource = _template
                .Replace("##SOURCE##", newInput)
                .Replace("##NS##", assmName)
                .Replace("##SCHEMA##", "") // schema is linked
                .Replace("##DB##", contextResult.Type.ToString());
            var e1 = sw.Elapsed.TotalMilliseconds;
            Console.WriteLine(programSource);
            sw.Reset();
            sw.Start();
            var result = _compiler.LoadType(programSource, assmName, contextResult.Reference);
            var method = result.Type.GetMethod("Run");
            var programInstance = Activator.CreateInstance(result.Type);
            var e2 = sw.Elapsed.TotalMilliseconds;
            sw.Reset();
            sw.Start();
            var res = method.Invoke(programInstance, new object[] { }) as DumpInternal;
            var e3 = sw.Elapsed.TotalMilliseconds;
            //res.Add("Performance", new { DbContext = e1, Loading = e2, Execution = e3 });
            return res;
        }

        public TemplateResult GetTemplate(QueryInput input) 
        {
            var srcToken = "##SOURCE##";
            var assmName = Guid.NewGuid().ToIdentifierWithPrefix("a");
            var schemaResult = _schemaService.GetSchemaSource(input.ConnectionString, assmName, withUsings: false);
            var schemaSrc = schemaResult.Schema;
            
            var src = _template
                .Replace("##NS##", assmName)
                .Replace("##DB##", "Proxy")
                .Replace("##SCHEMA##", schemaSrc);
                
            var srcLineOffset = -1;
            var lines = src.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            for(var i = lines.Length - 1; i > 0; i--) {
                if (lines[i].Contains(srcToken)) {
                    lines[i] = string.Empty;
                    srcLineOffset = i + 1;
                    break;
                }
            }
            var fullSrc = string.Join("\n", lines); // todo: newline constant?
            // the usage of the template should not require mapping the column value
            return new TemplateResult 
            {
                Template = fullSrc,
                Namespace = assmName,
                ColumnOffset = 0,
                LineOffset = srcLineOffset,
                DefaultQuery = string.Format("{0}.Take(100).Dump();\n\n", schemaResult.DefaultTable)
            };
        }

        string _template = @"using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using DumpType = System.Tuple<System.Collections.Generic.IEnumerable<System.Tuple<string, string>>, object>;
##SCHEMA##
namespace ##NS## 
{     
    public static class Dumper 
    {
        public static IDictionary<string, DumpType> _results;
        public static IDictionary<string, int> _counts;
        public static int _anonynousCount;

        /// <summary>QueryEngine.Inlined.Dumper</summary>
        public static T Dump<T>(this T o)
        {
            if (o != null)
            {
                var objType = o.GetType();
                var name = objType.Name;
                if (o is IEnumerable<object>) {
                    name = objType.GetTypeInfo().GenericTypeArguments[0].Name; 
                }
                _results.Add(FormatNameKey(name), Tuple.Create(TypeColumns((object)o), (object)o));
            }
            return o;
        }
        
        static IEnumerable<Tuple<string, string>> TypeColumns(object o) 
        {
            var t = o.GetType();
            if (o is IEnumerable<object>) 
            {
                t = t.GetTypeInfo().GenericTypeArguments[0];
            }
            var list = new List<Tuple<string, string>>();
            foreach(var m in t.GetMembers())
            {
                var propInfo = m as System.Reflection.PropertyInfo;
                if (propInfo != null)
                {
                    list.Add(Tuple.Create(m.Name, propInfo.GetGetMethod().ReturnType.Name));
                }
            }
            return list;
        }
        
        static string FormatNameKey(string name) 
        {
            var isAnon = name.Contains(""AnonymousType"");
            var count = -1;
            if (isAnon)
            {
                count = ++_anonynousCount;
            }
            else 
            {
                if (!_counts.ContainsKey(name))
                {
                    _counts.Add(name, 0);
                }
                count = _counts[name] + 1;
                _counts[name] = count;
            }
            var typeName = isAnon ? ""AnonymousType"" : name;
            var result =  string.Format(""{0} {1}"", typeName, count);
            return result;
        }
    }

    public class Main : ##DB##
    {
        public IDictionary<string, DumpType> Run()
        {
            // todo need something better
            Dumper._results = new Dictionary<string, DumpType>();
            Dumper._counts = new Dictionary<string, int>();
            Dumper._anonynousCount = 0;
            Query();
            return Dumper._results;
        }

        void Query()
        {
##SOURCE##
        }
    }
}
";
        static IEnumerable<Tuple<string, string>> TypeColumns(object o) 
        {
            var t = o.GetType();
            if (o is IEnumerable<object>) 
            {
                t = t.GetTypeInfo().GenericTypeArguments[0];
            }
            var list = new List<Tuple<string, string>>();
            foreach(var m in t.GetMembers())
            {
                var propInfo = m as System.Reflection.PropertyInfo;
                if (propInfo != null)
                {
                    list.Add(Tuple.Create(m.Name, propInfo.GetGetMethod().ReturnType.Name));
                }
            }
            return list;
        }
    }
}
