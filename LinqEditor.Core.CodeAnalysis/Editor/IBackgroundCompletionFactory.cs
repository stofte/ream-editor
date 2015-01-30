using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.CodeAnalysis.Editor
{
    public interface IBackgroundCompletionFactory
    {
        IBackgroundCompletion Create(string connectionString);
        void Release(IBackgroundCompletion backgroundCompletion);
    }
}
