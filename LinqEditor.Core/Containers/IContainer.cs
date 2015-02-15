using LinqEditor.Core.Models.Editor;
using System.Threading;

namespace LinqEditor.Core.Containers
{
    public interface IContainer
    {
        LoadAppDomainResult Initialize(byte[] assembly);
        LoadAppDomainResult Initialize(string assemblyPath);
        LoadAppDomainResult Initialize();
        ExecuteResult Execute(byte[] assembly);
        ExecuteResult Execute(byte[] assembly, CancellationToken ct);
        ExecuteResult Execute(string path);
        ExecuteResult Execute(string path, CancellationToken ct);
    }
}
