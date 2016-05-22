namespace QueryEngine.Models
{
    public class TemplateResult 
    {
        public string Namespace { get; set; }
        public string Template { get; set; }
        public string Header { get; set; }
        public string Footer { get ; set; }
        
        public int ColumnOffset { get; set; }
        
        public int LineOffset { get; set; }
        
        public string DefaultQuery { get; set; }
    }
}
