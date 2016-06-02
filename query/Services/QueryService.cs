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

        public QueryService(CompileService compiler, DatabaseContextService databaseContextService, SchemaService schemaService)
        {
            _compiler = compiler;
            _databaseContextService = databaseContextService;
            _schemaService = schemaService;
        }

        public DumpInternal ExecuteQuery(QueryInput input)
        {
            var sw = new Stopwatch();
            sw.Start();
            var contextResult = _databaseContextService.GetDatabaseContext(input.ConnectionString);
            var assmName = Guid.NewGuid().ToIdentifierWithPrefix("a");
            var programSource = _template
                .Replace("##SOURCE##", input.Text)
                .Replace("##NS##", assmName)
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
            var assmName = Guid.NewGuid().ToIdentifierWithPrefix("a");
            var schemaResult = _schemaService.GetSchemaSource(input.ConnectionString, assmName, withUsings: false);
            var schemaSrc = schemaResult.Schema;
            
            var src = _template
                .Replace("##NS##", assmName)
                .Replace("##DB##", "Proxy");
                
            var srcToken = "##SOURCE##";
            var regex = new Regex(@"$", RegexOptions.Multiline);
            var srcIdx = src.IndexOf(srcToken);
            var srcOffset = src.Substring(0, srcIdx);
            var ms = regex.Matches(srcOffset);
            src = src + schemaSrc;
            var fullSrc = src.Replace(srcToken, "");
            var header = src.Substring(0, srcIdx);
            var footer = src.Substring(srcIdx + srcToken.Length);
            // the usage of the template should not require mapping the column value
            return new TemplateResult 
            {
                Template = fullSrc,
                Header = header,
                Footer = footer,
                Namespace = assmName,
                ColumnOffset = 0,
                LineOffset = ms.Count,
                DefaultQuery = string.Format("{0}.Take(100).Dump();\n\n", schemaResult.DefaultTable)
            };
        }

        string _template = @"namespace ##NS##
{
    using System;
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
    using DumpType = System.Tuple<System.Collections.Generic.IEnumerable<
        System.Tuple<string, string>>, object>;
    
    public class Main : ##DB##
    {
        public IDictionary<string, DumpType> Run()
        {
            // todo need something better
            Dumper._results = new Dictionary<string, DumpType>();
            Dumper._count = 0;
            Query();
            return Dumper._results;
        }

        void Query()
        {
##SOURCE##
        }
    }
    
    public static class Dumper 
    {
        public static IDictionary<string, DumpType> _results;
        public static int _count;

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
                _results.Add(PrettyAnonymous(name), Tuple.Create(TypeColumns((object)o), (object)o));
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
                if (m.MemberType == MemberTypes.Field || m.MemberType == MemberTypes.Property)
                {
                    list.Add(Tuple.Create(m.Name, m.DeclaringType.Name));
                }
            }
            return list;
        }
        
        static string PrettyAnonymous(string name) 
        {
            return name.Contains(""AnonymousType"") ? string.Format(""AnonymousType {0}"", ++_count) : name;
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
                if (m.MemberType == MemberTypes.Field || m.MemberType == MemberTypes.Property)
                {
                    list.Add(Tuple.Create(m.Name, m.DeclaringType.Name));
                }
            }
            return list;
        }
    }
}
