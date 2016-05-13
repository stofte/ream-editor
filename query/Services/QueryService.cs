namespace QueryEngine.Services
{
    using System;
    using System.Diagnostics;
    using System.Collections.Generic;
    using System.Reflection;
    using QueryEngine.Models;
    using System.Text.RegularExpressions;
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

        public IDictionary<string, object> ExecuteQuery(QueryInput input)
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
            sw.Reset();
            sw.Start();
            var result = _compiler.LoadType(programSource, assmName, contextResult.Reference);
            var method = result.Type.GetMethod("Run");
            var programInstance = Activator.CreateInstance(result.Type);
            var e2 = sw.Elapsed.TotalMilliseconds;
            sw.Reset();
            sw.Start();
            var res = method.Invoke(programInstance, new object[] { }) as IDictionary<string, object>;
            var e3 = sw.Elapsed.TotalMilliseconds;
            //res.Add("Performance", new { DbContext = e1, Loading = e2, Execution = e3 });
            return res;
        }

        public TemplateResult GetTemplate(QueryInput input) 
        {
            var assmName = Guid.NewGuid().ToIdentifierWithPrefix("a");
            var schemaSrc = _schemaService.GetSchemaSource(input.ConnectionString, assmName, withUsings: false);
            
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
            Console.WriteLine("regex matches", ms.Count);
            // the usage of the template should not require mapping the column value
            return new TemplateResult {
                Template = fullSrc,
                Header = header,
                Footer = footer,
                Namespace = assmName,
                ColumnOffset = 0,
                LineOffset = ms.Count // todo magic bullshit
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

namespace ##NS##
{
    public class Main : ##DB##
    {
        public IDictionary<string, object> Run()
        {
            // todo need something better
            Dumper._results = new Dictionary<string, object>();
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
        public static IDictionary<string, object> _results;
        public static int _count;

        public static T Dump<T>(this T o)
        {
            // since the context is lost when returning, we tolist anything we dump
            var name = o.GetType().Name;
            object result = null;
            
            if (o is IEnumerable<object>)
            {
                name = o.GetType().GetTypeInfo().GenericTypeArguments[0].Name;
                var ol = o as IEnumerable<object>;
                result = ol.ToList();
            }
            else
            {
                result = o;
            }
            var displayName = string.Format(""{0} {1}"", PrettyAnonymous(name), ++_count);
            _results.Add(displayName, result);
            return o;
        }

        static string PrettyAnonymous(string name) 
        {
            return name.Contains(""AnonymousType"") ? ""AnonymousType"" : name;
        }
    }
}
";
    }
}
