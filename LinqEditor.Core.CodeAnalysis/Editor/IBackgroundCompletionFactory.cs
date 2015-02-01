
namespace LinqEditor.Core.CodeAnalysis.Editor
{
    public interface IBackgroundCompletionFactory
    {
        IBackgroundCompletion Create(string connectionString);
        void Release(IBackgroundCompletion backgroundCompletion);
    }
}
