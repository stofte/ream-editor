
namespace LinqEditor.Core.CodeAnalysis.Models
{
    public class LocationSpan
    {
        public int StartLine { get; set; }
        public int StartColumn { get; set; }

        public int EndLine { get; set; }
        public int EndColumn { get; set; }
    }
}
