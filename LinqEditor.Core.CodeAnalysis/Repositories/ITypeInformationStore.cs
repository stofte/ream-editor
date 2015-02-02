using LinqEditor.Core.CodeAnalysis.Models;
using Microsoft.CodeAnalysis;

namespace LinqEditor.Core.CodeAnalysis.Repositories
{
    public interface ITypeInformationStore
    {
        void InitializeModel(SemanticModel model, SyntaxTree tree);
        TypeInformation GetInformation(TypeInfo typeInfo, SymbolInfo symbolInfo);
    }
}
