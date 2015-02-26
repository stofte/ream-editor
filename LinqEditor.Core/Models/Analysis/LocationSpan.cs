
using System;
using System.Text;
namespace LinqEditor.Core.Models.Analysis
{
    public class LocationSpan
    {
        public LocationSpan()
        {
            StartIndex = -1;
            StartLine = -1;
            StartColumn = -1;
            EndIndex = -1;
            EndLine = -1;
            EndColumn = -1;
        }

        public int StartIndex { get; set; }
        public int StartLine { get; set; }
        public int StartColumn { get; set; }

        public int EndIndex { get; set; }
        public int EndLine { get; set; }
        public int EndColumn { get; set; }

        public override string ToString()
        {
            return string.Format("{0},{1} -> {2},{3}", StartLine, StartColumn, EndLine, EndColumn);
        }

        public LocationSpan Offset(int index, int line)
        {
            return new LocationSpan
            {
                StartIndex = StartIndex + index,
                StartLine = StartLine + line,
                StartColumn = StartColumn,
                EndIndex = EndIndex + index,
                EndLine = EndLine + line,
                EndColumn = EndColumn
            };
        }

        public string GetText(string sourceCode)
        {
            var lines = sourceCode.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            var sb = new StringBuilder();
            for (var i = 0; i < lines.Length; i++)
            {
                if (StartLine - 1 == i && EndLine == StartLine)
                {
                    sb.Append(lines[i].Substring(StartColumn - 1, EndColumn - StartColumn));
                }
                else if (StartLine - 1 == i)
                {
                    sb.AppendLine(lines[i].Substring(StartColumn - 1));
                }
                else if (StartLine - 1 < i && EndLine - 1 > i)
                {
                    sb.AppendLine(lines[i]);
                }
                else if (EndLine - 1 == i)
                {
                    sb.Append(lines[i].Substring(0, EndColumn - 1));
                }
            }
            return sb.ToString();
        }
    }
}