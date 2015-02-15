
namespace LinqEditor.Core.Containers
{
    public interface IIsolatedDatabaseContainerFactory
    {
        IIsolatedDatabaseContainer Create();
        void Release(IIsolatedDatabaseContainer instance);
    }
}
