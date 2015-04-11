using LinqEditor.Core.CodeAnalysis.Services;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LinqEditor.Core.CodeAnalysis.Compiler
{
    /// <summary>
    /// Defines the static references required for compiling code
    /// </summary>
    public static class CompilerReferences
    {
        public static Assembly[] GetCoreAssemblies()
        {
            return new Assembly[] 
            {
                typeof(System.Object).Assembly,// mscorlib.dll
                typeof(System.ComponentModel.Component).Assembly, // System.Core.dll 4.0
                typeof(System.Data.DataColumn).Assembly, // System.Data.dll
                typeof(System.Xml.XmlDocument).Assembly, // System.Xml.dll
                typeof(System.Linq.Enumerable).Assembly, // System.Core.dll 3.5 for some reason also needed?
            };
        }

        public static Assembly[] GetCustomAssemblies()
        {
            return new Assembly[]
            {
                typeof(IQToolkit.QueryProvider).Assembly,  // IQToolkit.dll
                typeof(IQToolkit.Data.DbEntityProvider).Assembly,  // IQToolkit.Data.dll
                typeof(IQToolkit.Data.SqlClient.TSqlLanguage).Assembly, // IQToolkit.Data.SqlClient.dll
                typeof(LinqEditor.Core.Generated.Dumper).Assembly, // LinqEditor.Core.dll
            };
        }

        public static IEnumerable<MetadataReference> GetStandardReferencesWithIncludes(string[] references)
        {
            return GetStandardReferences(includeDocumentation: false)
                .Concat(references.Select(x => MetadataReference.CreateFromFile(x)))
                .ToArray();
        }

        public static MetadataReference[] GetStandardReferences(bool includeDocumentation = false)
        {
            var assems = new List<MetadataReference>();
            // todo: supposedly passing in CustomDocumentationProvider here would
            // cause the api to return the docs through the api (GetDocumentationCommentXml)
            // but that doesn't seem to do anything. so a workaround is to just serve the comments
            // from a custom service (DocumentationService), which builds a list of 
            // CustomDocumentationProvider by using the above GetAssemblies methods.
            return assems
                .Concat(GetCoreAssemblies().Select(x => MetadataReference.CreateFromAssembly(x)))
                .Concat(GetCustomAssemblies().Select(x => MetadataReference.CreateFromAssembly(x)))
                .ToArray();
        }
    }
}
