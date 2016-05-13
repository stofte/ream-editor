namespace QueryEngine.Services
{
    using System;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Logging;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.EntityFrameworkCore.Design.Internal;
    
    public class SchemaService 
    {
        public string GetSchemaSource(string connectionString, string assemblyNamespace, bool withUsings = true) 
        {
            var loggerFactory = new LoggerFactory().AddConsole();

            var ssTypeMap = new Microsoft.EntityFrameworkCore.Storage.Internal.SqlServerTypeMapper();
            var ssDbFac = new SqlServerDatabaseModelFactory(loggerFactory: loggerFactory);
            var ssScaffoldFac = new SqlServerScaffoldingModelFactory(
                loggerFactory: loggerFactory,
                typeMapper: ssTypeMap,
                databaseModelFactory: ssDbFac
            );

            var ssAnnotationProvider = new Microsoft.EntityFrameworkCore.Metadata.SqlServerAnnotationProvider();
            var csUtils = new CSharpUtilities();
            var scaffUtils = new ScaffoldingUtilities();

            var confFac = new ConfigurationFactory(
                extensionsProvider: ssAnnotationProvider,
                cSharpUtilities: csUtils,
                scaffoldingUtilities: scaffUtils
            );
            var fs = new InMemoryFileService();
            var sb = new StringBuilderCodeWriter(
                fileService: fs,
                dbContextWriter: new DbContextWriter(
                    scaffoldingUtilities: scaffUtils,
                    cSharpUtilities: csUtils
                ),
                entityTypeWriter: new EntityTypeWriter(cSharpUtilities: csUtils, scaffoldingUtilities: scaffUtils)
            );

            var rGen = new ReverseEngineeringGenerator(
                loggerFactory: loggerFactory,
                scaffoldingModelFactory: ssScaffoldFac,
                configurationFactory: confFac,
                codeWriter: sb
            );

            var outputPath = @"C:\temp";
            var programName = "Ctx";
            var conf = new ReverseEngineeringConfiguration 
            {
                ConnectionString = connectionString,
                ContextClassName = programName,
                ProjectPath = "na",
                ProjectRootNamespace = assemblyNamespace,
                OutputPath = outputPath
            };

            var output = new StringBuilder();
            var resFiles = rGen.GenerateAsync(conf);
            resFiles.Wait();
            
            var ctx = CreateContext(fs.RetrieveFileContents(outputPath, programName + ".cs"), isLibrary: withUsings);
            Console.WriteLine("CreateContext", ctx);
            if (!withUsings) 
            {
                ctx = StripHeaderLines(3, ctx);
            }
            else
            {
                output.Append(_refs);
            }
            output.Append(ctx);
            foreach(var fpath in resFiles.Result.EntityTypeFiles)
            {
                output.Append(StripHeaderLines(4, fs.RetrieveFileContents(outputPath, System.IO.Path.GetFileName(fpath))));
            }
            return output.ToString();
        }

        string StripHeaderLines(int lines, string contents) 
        {
            return string.Join("\n", contents.Split('\n').Skip(lines));
        }

        string CreateContext(string ctx, bool isLibrary = true) 
        {
            var idx1 = ctx.IndexOf("{");
            var idx = ctx.IndexOf("{", idx1 + 1) + 1;
            var pre = ctx.Substring(0, idx);
            var post = ctx.Substring(idx);
            var newCtx = pre + _instance + post.Substring(0, post.Length - 2);
            var regex = new Regex(@"^\s*public virtual DbSet\<([^>]*).*$", RegexOptions.Multiline);
            var tables = regex.Matches(ctx);
            var proxyCtx = _proxyPre.Replace("##PROXY##", isLibrary ? "Main" : "Proxy");
            foreach(Match t in tables)
            {
                var proxy = "get { return Ctx.Instance.Value." + t.Groups[1].Value + "; }";
                proxyCtx += t.Value.Replace("get; set;", proxy);
            }
            return newCtx + proxyCtx + _proxyPost;
        }

        string _refs = @"
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
";

        string _instance = @"
        public static Lazy<Ctx> Instance = new Lazy<Ctx>(() => new Ctx());
";

        string _proxyPre = @"
    public class ##PROXY##
    {
";

        string _proxyPost = @"
    }
}
";
    }
}
