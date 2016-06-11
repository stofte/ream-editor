namespace QueryEngine.Models
{
    public class QueryInput
    {
        public DatabaseProviderType ServerType { get; set; }
        public string ConnectionString { get; set; }
        public string Namespace { get; set; }
        public string Text { get; set; }
    }
}
