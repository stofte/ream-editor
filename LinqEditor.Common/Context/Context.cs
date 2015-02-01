using System;

namespace LinqEditor.Common.Context
{
    public class Context : IContext
    {
        private string _path;
        private string _schema;
        
        public event Action<string, string> ContextUpdated;


        public void UpdateContext(string assemblyPath, string schemaNamespace)
        {
            _path = assemblyPath;
            _schema = schemaNamespace;
            ContextUpdated(_path, _schema);
        }
    }
}
