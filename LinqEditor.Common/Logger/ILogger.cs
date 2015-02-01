using System.Runtime.CompilerServices;

namespace LinqEditor.Common.Logger
{
    public interface ILogger
    {
        void Write(LogCategory category, string message, [CallerMemberName] string callingMemberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0);
    }
}
