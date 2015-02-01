using System;

namespace LinqEditor.Common.Context
{
    public interface IContext
    {
        void UpdateContext(string assemblyPath, string schemaNamespace);
        event Action<string, string> ContextUpdated;
    }
}
