namespace QueryEngine.Models
{
    using System;

    public class QueryResult 
    {
        public Guid Id { get; set; }
        public DateTime Created { get; set; } 
        public object Results { get; set; }
    }
}
