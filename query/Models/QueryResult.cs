namespace QueryEngine.Models
{
    using System;
    using System.Collections.Generic;

    public class QueryResult 
    {
        public Guid Id { get; set; }
        public DateTime Created { get; set; } 
        public IDictionary<string, object> Results { get; set; }
    }
}
