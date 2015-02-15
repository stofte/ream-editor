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
        public ExecuteResult()
        {
            Tables = new List<DataTable>();
            Warnings = new List<Warning>();
            Errors = new List<Error>();
            CodeOutput = string.Empty;
            QueryText = string.Empty;
        }

        public long DurationMs { get; set; }
        public IEnumerable<DataTable> Tables { get; set; }
        public IEnumerable<Warning> Warnings { get; set; }
        public IEnumerable<Error> Errors { get; set; }
        public Exception Exception { get; set; }
        public Exception InternalException { get; set; }
        public bool Success { get; set; }
        public string QueryText { get; set; }

        public ProgramType Kind { get; set; }
        public string CodeOutput { get; set; }

        public override string ToString()
        {
            var str = string.Format("success:{0}, codeoutput:{1}", Success, CodeOutput);
            return str;
        }
    }
}
