namespace QueryEngine.Models
{
    public class QueryInput
    {
        public string ConnectionString { get; set; }
        public string Namespace { get; set; }
        public string Text { get; set; }
    }
}
