using LinqEditor.Core.CodeAnalysis.Models;
using System.Collections.Generic;

namespace LinqEditor.Core.CodeAnalysis.Services
{
    public interface IEditor
    {
        void Initialize(string assemblyPath = null, string schemaNamespace = null);
        EditContext UpdateSource(string sourceFragment, int updateIndex);
        IEnumerable<SuggestionEntry> MemberCompletions();
    }
}
