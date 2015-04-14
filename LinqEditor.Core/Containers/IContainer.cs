using LinqEditor.Core.Models.Editor;

namespace LinqEditor.Core.Containers
{
    public interface IContainer
    {
        LoadAppDomainResult Initialize(byte[] assembly, params object[] args);
        LoadAppDomainResult Initialize(string assemblyPath, params object[] args);
        LoadAppDomainResult Initialize(params object[] args);
        ExecuteResult Execute(byte[] assembly);
        ExecuteResult Execute(string path);
    }
}
