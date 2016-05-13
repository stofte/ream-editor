namespace QueryEngine.Handlers
{
    using System;
    using System.Threading.Tasks;
    using System.Runtime.Serialization.Json;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using QueryEngine.Services;
    using QueryEngine.Models;

    public class ExecuteQueryHandler : BaseHandler<QueryResult, QueryInput>
    {
        QueryService _service;

        public ExecuteQueryHandler(RequestDelegate next, QueryService service) : base(next) 
        {
            _service = service; 
        }

        protected override bool Handle(string path)
        {
            return path.Contains("/executequery");
        }

        protected override QueryResult Execute(QueryInput input)
        {
            var res = _service.ExecuteQuery(input);
            return new QueryResult
            {
                Results = res
            };
        }
    }
}
