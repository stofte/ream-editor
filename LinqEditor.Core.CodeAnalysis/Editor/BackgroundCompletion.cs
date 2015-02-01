using LinqEditor.Core.Templates;
using System.Threading.Tasks;

namespace LinqEditor.Core.CodeAnalysis.Editor
{
    public class BackgroundCompletion : Completion, IBackgroundCompletion
    {
        public BackgroundCompletion(ITemplateService templateService)
            : base(templateService) { }

        public async Task InitializeAsync(string assemblyPath = null, string schemaNamespace = null)
        {
            await Task.Run(() => Initialize(assemblyPath, schemaNamespace));
        }

        public async Task UpdateFragmentAsync(string fragment)
        {
            await Task.Run(() => UpdateFragment(fragment));
        }

        public async Task<Models.SuggestionList> MemberAccessExpressionCompletionsAsync(int fragmentIndex)
        {
            return await Task.Run(() => MemberAccessExpressionCompletions(fragmentIndex));
        }
    }
}
