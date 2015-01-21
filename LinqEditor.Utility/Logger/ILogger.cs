using LinqEditor.Utility.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Utility
{
    public interface ILogger
    {
        void Write(LogCategory category, string message, [CallerMemberName] string callingMemberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0);
    }
}
