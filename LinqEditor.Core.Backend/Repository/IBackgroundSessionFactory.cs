
namespace LinqEditor.Core.Backend.Repository
{
    public interface IBackgroundSessionFactory
    {
        IBackgroundSession Create(string connectionString);
        void Release(IBackgroundSession backgroundSession);
    }
}
