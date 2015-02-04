using LinqEditor.Core.Models.Analysis;
using System;
using System.Collections.Generic;
using System.Data;

namespace LinqEditor.Core.Models.Editor
{
    // todo: split into code<->backend and ui<->backend parts
    [Serializable]
    public class ExecuteResult
    {
        public long Duration { get; set; }
        public IEnumerable<DataTable> Tables { get; set; }
        public IEnumerable<Warning> Warnings { get; set; }
        public IEnumerable<Error> Errors { get; set; }
        public Exception Exception { get; set; }
        public Exception InternalException { get; set; }
        public bool Success { get; set; }
        public string QueryText { get; set; }

        public ProgramType Kind { get; set; }
        public string CodeOutput { get; set; }
    }
}
