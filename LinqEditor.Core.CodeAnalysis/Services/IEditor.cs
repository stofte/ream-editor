using LinqEditor.Core.CodeAnalysis.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.CodeAnalysis.Services
{
    public interface IEditor
    {
        void Initialize(string assemblyPath = null, string schemaNamespace = null);
        EditContext UpdateSource(string sourceFragment, int updateIndex);
        IEnumerable<SuggestionEntry> MemberCompletions();
    }
}
