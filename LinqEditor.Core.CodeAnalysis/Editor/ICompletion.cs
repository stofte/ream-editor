using LinqEditor.Core.CodeAnalysis.Models;

namespace LinqEditor.Core.CodeAnalysis.Editor
{
    public interface ICompletion
    {
        void Initialize(string assemblyPath = null, string schemaNamespace = null);
        void UpdateFragment(string fragment);
        /// <summary>
        /// Suggests completions for the member access expression ("x.")
        /// </summary>
        /// <param name="fragmentIndex">Index of the expression.</param>
        /// <returns></returns>
        SuggestionList MemberAccessExpressionCompletions(int fragmentIndex);
    }
}
