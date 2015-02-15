
namespace LinqEditor.Core.Containers
{
    public interface IIsolatedCodeContainerFactory
    {
        IIsolatedCodeContainer Create();
        void Release(IIsolatedCodeContainer instance);
    }
}
