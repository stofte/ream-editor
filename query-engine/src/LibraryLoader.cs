namespace QueryEngine
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Runtime.Loader;

    public class LibraryLoader : AssemblyLoadContext
    {
        public static Lazy<LibraryLoader> Instance = new Lazy<LibraryLoader>(() => new LibraryLoader());

        public Stream AssemblyStream;
        
        protected override Assembly Load(AssemblyName assemblyName)
        {
            return base.LoadFromStream(AssemblyStream);
        }
    }
}
