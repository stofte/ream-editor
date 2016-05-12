namespace QueryEngine.Models
{
    using System;
    using System.Reflection;
    using Microsoft.CodeAnalysis;

    public class CompileResult
    {
        public Type Type { get; set; }
        public Assembly Assembly { get; set; }
        public MetadataReference Reference { get; set; }
    }
}
