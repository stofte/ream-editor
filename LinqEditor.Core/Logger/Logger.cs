using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace LinqEditor.Core.Logger
{
    public class Logger : ILogger
    {
        public void Write(LogCategory category, string message, [CallerMemberName] string callingMemberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            Trace.WriteLine(message, category.ToString());
        }
    }
}
