namespace QueryEngine.Models
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    public class QueryResult 
    {
        public IDictionary<string, object> Results { get; set; }
    }
}
