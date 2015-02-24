using System.Runtime.CompilerServices;

namespace LinqEditor.Core.Helpers
{
    public static class DebugLogger
    {
        public static void Log(
            object msg,
            [CallerMemberName] string member = "",
            [CallerFilePath] string sourcePath = "",
            [CallerLineNumber] int sourceLine = 0)
        {
#if DEBUG

            var id = System.Threading.Thread.CurrentThread.ManagedThreadId.ToString();
            var str = string.Format("{3} [{4} {0}:{1}:{2}]", sourcePath, sourceLine, member, msg, id);
            System.Diagnostics.Debug.WriteLine(str);
#endif
        }
    }
}
