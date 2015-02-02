using LinqEditor.Core.CodeAnalysis.Models;
using System.Collections.Generic;

namespace LinqEditor.Core.CodeAnalysis.Services
{
    public interface IEditor
    {
        EditContext UpdateSource(string sourceFragment, int updateIndex);
        IEnumerable<SuggestionEntry> MemberCompletions();
    }
}
