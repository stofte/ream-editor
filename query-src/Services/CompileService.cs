namespace QueryEngine.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using System.Reflection;
    using System.Runtime.Loader;
    using Microsoft.Extensions.Logging;
    using Microsoft.DotNet.ProjectModel.Workspaces;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Emit;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using QueryEngine.Models;
    
    public class CompileService
    {
        SchemaService _schemaService;
        string _projectjsonPath;
        IEnumerable<MetadataReference> _references;

        public CompileService(SchemaService schemaService) 
        {
            _schemaService = schemaService;
            _projectjsonPath = Directory.GetCurrentDirectory();
            AssemblyLoadContext.InitializeDefaultContext(LibraryLoader.Instance.Value);
        }

        public IEnumerable<MetadataReference> GetReferences() 
        {
            if (_references == null)
            {
                _references = new ProjectJsonWorkspace(_projectjsonPath)
                    .CurrentSolution.Projects.First().MetadataReferences;
            }
            return _references;
        }
        
        public CompileResult LoadType(string source, string assemblyName, MetadataReference context = null)
        {
            var references = GetReferences();
            if (context != null)
            {
                references = references.Concat(new MetadataReference[] { context });
            }

            var compilerOptions = new CSharpCompilationOptions(outputKind: OutputKind.DynamicallyLinkedLibrary);
            var trees = new SyntaxTree[] {
                CSharpSyntaxTree.ParseText(source),
            };

            var compilation = CSharpCompilation.Create(assemblyName)
                .WithOptions(compilerOptions)
                .WithReferences(references)
                .AddSyntaxTrees(trees);

            var stream = new MemoryStream();
            var compilationResult = compilation.Emit(stream, options: new EmitOptions());
            stream.Position = 0;
            if (!compilationResult.Success) 
            {
                foreach(var r in compilationResult.Diagnostics.Where(x => x.Severity == DiagnosticSeverity.Error)) 
                {
                    Console.WriteLine("Error: {0}", r);
                }
            }
            LibraryLoader.Instance.Value.AssemblyStream = stream;
            var asm = LibraryLoader.Instance.Value.LoadFromAssemblyName(new AssemblyName(assemblyName));
            var programType = asm.GetTypes().Single(t => t.Name == "Main");
            stream.Position = 0;

            return new CompileResult 
            {
                Type = programType,
                Assembly = asm,
                Reference = MetadataReference.CreateFromStream(stream)
            };
        }
    }
}
