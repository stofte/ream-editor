using System;

namespace LinqEditor.Core.Models.Editor
{
    [Serializable]
    public class LoadAppDomainResult
    {
        public long DurationMs { get; set; }
        public Exception Error { get; set; }
    }
}
