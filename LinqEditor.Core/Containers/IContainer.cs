using LinqEditor.Core.Models.Editor;

namespace LinqEditor.Core.Containers
{
    public interface IContainer
    {
        LoadAppDomainResult Initialize(byte[] assembly);
        LoadAppDomainResult Initialize(string assemblyPath);
        LoadAppDomainResult Initialize();
        ExecuteResult Execute(byte[] assembly);
        ExecuteResult Execute(string path);
    }
}
