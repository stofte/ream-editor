using LinqEditor.Core.CodeAnalysis.Models;
using System.Threading.Tasks;

namespace LinqEditor.Core.CodeAnalysis.Editor
{
    public interface IBackgroundCompletion
    {
        Task InitializeAsync(string assemblyPath = null, string schemaNamespace = null);
        Task UpdateFragmentAsync(string fragment);
        /// <summary>
        /// Suggests completions for the member access expression ("x.")
        /// </summary>
        /// <param name="fragmentIndex">Index of the expression.</param>
        /// <returns></returns>
        Task<SuggestionList> MemberAccessExpressionCompletionsAsync(int fragmentIndex);
    }
}
