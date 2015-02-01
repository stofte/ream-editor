using LinqEditor.Core.CodeAnalysis.Models;
using Microsoft.CodeAnalysis;

namespace LinqEditor.Core.CodeAnalysis.Repositories
{
    public interface ITypeInformationStore
    {
        TypeInformation GetInformation(TypeInfo typeInfo);
    }
}
