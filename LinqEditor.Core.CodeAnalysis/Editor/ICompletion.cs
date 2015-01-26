using LinqEditor.Core.CodeAnalysis.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.CodeAnalysis.Editor
{
    public interface ICompletion
    {
        void Initialize(string assemblyPath = null);
        void UpdateFragment(string fragment);
        /// <summary>
        /// Suggests completions for the member access expression ("x.")
        /// </summary>
        /// <param name="fragmentIndex">Index of the expression.</param>
        /// <returns></returns>
        SuggestionList MemberAccessExpressionCompletions(int fragmentIndex);
    }
}
