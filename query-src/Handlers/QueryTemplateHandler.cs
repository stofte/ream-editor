namespace QueryEngine.Handlers
{
    using System;
    using System.Threading.Tasks;
    using System.Runtime.Serialization.Json;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using QueryEngine.Services;
    using QueryEngine.Models;

    public class QueryTemplateHandler : BaseHandler<TemplateResult, QueryInput>
    {
        QueryService _queryService;

        public QueryTemplateHandler(RequestDelegate next, QueryService queryService) : base(next) 
        {
            _queryService = queryService;
        }

        protected override bool Handle(string path)
        {
            return path.Contains("/querytemplate");
        }

        protected override TemplateResult Execute(QueryInput input)
        {
            return _queryService.GetTemplate(input);
        }
    }
}