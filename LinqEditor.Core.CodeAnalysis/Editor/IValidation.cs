using LinqEditor.Core.CodeAnalysis.Models;

namespace LinqEditor.Core.CodeAnalysis.Editor
{
    public interface IValidation
    {
        void Initialize(string source);
        void UpdateSource(string source);
        ValidationResults Validate();
    }
}
