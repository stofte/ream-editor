namespace QueryEngine.Services
{
    using System;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Linq;
    using Microsoft.Extensions.Logging;
    using Microsoft.EntityFrameworkCore.Scaffolding;
    using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
    using QueryEngine.Models;

    public class SchemaService 
    {
        string _tempFolder;

        public SchemaService() 
        {
            _tempFolder = Environment.GetEnvironmentVariable("TEMP");
        }

        public SchemaResult GetSchemaSource(string connectionString, DatabaseProviderType type, string assemblyNamespace, bool withUsings = true) 
        {
            if (type == DatabaseProviderType.SqlServer) 
            {
                return GetSchemaSource(connectionString, assemblyNamespace, withUsings);
            }
            else 
            {
                return GetNpgSqlSchemaSource(connectionString, assemblyNamespace, withUsings);
            }
        }

        SchemaResult GetNpgSqlSchemaSource(string connectionString, string assemblyNamespace, bool withUsings = true) 
        {
            var loggerFactory = new LoggerFactory().AddConsole();
            var typeMapper = new Microsoft.EntityFrameworkCore.Storage.Internal.NpgsqlTypeMapper();
            var dbFac = new NpgsqlDatabaseModelFactory(loggerFactory: loggerFactory);
            var scaffoldFac = new NpgsqlScaffoldingModelFactory(
                loggerFactory: loggerFactory,
                typeMapper: typeMapper,
                databaseModelFactory: dbFac
            );
            var annotationProvider = new Microsoft.EntityFrameworkCore.Metadata.NpgsqlAnnotationProvider();
            var csUtils = new CSharpUtilities();
            var scaffUtils = new ScaffoldingUtilities();

            var confFac = new ConfigurationFactory(
                extensionsProvider: annotationProvider,
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
                scaffoldingModelFactory: scaffoldFac,
                configurationFactory: confFac,
                codeWriter: sb
            );

            var programName = "Ctx";
            var conf = new ReverseEngineeringConfiguration 
            {
                ConnectionString = connectionString,
                ContextClassName = programName,
                ProjectPath = "na",
                ProjectRootNamespace = assemblyNamespace,
                OutputPath = _tempFolder
            };
            
            var output = new StringBuilder();
            var resFiles = rGen.GenerateAsync(conf);
            resFiles.Wait();
            
            var dbCtx = CreateContext(fs.RetrieveFileContents(_tempFolder, programName + ".cs"), isLibrary: withUsings);
            var ctx = dbCtx.Item1;
            if (!withUsings) 
            {
                var x = 
                ctx = StripHeaderLines(3, ctx);
            }
            else
            {
                output.Append(_refs);
            }
            // remove the entity generated warning about injected connection strings
            ctx = Regex.Replace(ctx, @"#warning.*", "");
            output.Append(ctx);
            foreach(var fpath in resFiles.Result.EntityTypeFiles)
            {
                output.Append(StripHeaderLines(4, fs.RetrieveFileContents(_tempFolder, System.IO.Path.GetFileName(fpath))));
            }
            
            return new SchemaResult 
            {
                Schema = output.ToString(),
                DefaultTable = dbCtx.Item2
            };
        }

        SchemaResult GetSchemaSource(string connectionString, string assemblyNamespace, bool withUsings = true) 
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

            var programName = "Ctx";
            var conf = new ReverseEngineeringConfiguration 
            {
                ConnectionString = connectionString,
                ContextClassName = programName,
                ProjectPath = "na",
                ProjectRootNamespace = assemblyNamespace,
                OutputPath = _tempFolder
            };

            var output = new StringBuilder();
            var resFiles = rGen.GenerateAsync(conf);
            resFiles.Wait();
            
            var dbCtx = CreateContext(fs.RetrieveFileContents(_tempFolder, programName + ".cs"), isLibrary: withUsings);
            var ctx = dbCtx.Item1;
            if (!withUsings) 
            {
                var x = 
                ctx = StripHeaderLines(3, ctx);
            }
            else
            {
                output.Append(_refs);
            }
            // remove the entity generated warning about injected connection strings
            ctx = Regex.Replace(ctx, @"#warning.*", "");
            output.Append(ctx);
            foreach(var fpath in resFiles.Result.EntityTypeFiles)
            {
                output.Append(StripHeaderLines(4, fs.RetrieveFileContents(_tempFolder, System.IO.Path.GetFileName(fpath))));
            }
            
            return new SchemaResult 
            {
                Schema = output.ToString(),
                DefaultTable = dbCtx.Item2
            };
        }

        string StripHeaderLines(int lines, string contents) 
        {
            return string.Join("\n", contents.Split('\n').Skip(lines));
        }

        Tuple<string, string> CreateContext(string ctx, bool isLibrary = true) 
        {
            var idx1 = ctx.IndexOf("{");
            var idx = ctx.IndexOf("{", idx1 + 1) + 1;
            var pre = ctx.Substring(0, idx);
            var post = ctx.Substring(idx);
            var newCtx = pre + _instance + post.Substring(0, post.Length - 2);
            var regex = new Regex(@"^\s*public virtual DbSet\<([^>]*).*$", RegexOptions.Multiline);
            var tables = regex.Matches(ctx);
            var proxyCtx = _proxyPre.Replace("##PROXY##", isLibrary ? "Main" : "Proxy");
            var firstTable = string.Empty;
            foreach(Match t in tables)
            {
                if (string.IsNullOrEmpty(firstTable)) 
                {
                    firstTable = t.Groups[1].Value.ToString();
                }
                var proxy = "get { return Ctx.Instance.Value." + t.Groups[1].Value + "; }";
                proxyCtx += t.Value.Replace("get; set;", proxy);
            }
            return Tuple.Create(newCtx + proxyCtx + _proxyPost, firstTable);
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
