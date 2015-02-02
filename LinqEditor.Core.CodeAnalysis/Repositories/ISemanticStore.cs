using LinqEditor.Core.CodeAnalysis.Models;
using Microsoft.CodeAnalysis;

namespace LinqEditor.Core.CodeAnalysis.Repositories
{
    public interface ISemanticStore
    {
        void Initialize(SemanticModel model);
        void Update(SemanticModel model);
        void UpdateWithEdit(SemanticModel model, int updateIndex);
        TypeInformation GetInformation();
    }
}
