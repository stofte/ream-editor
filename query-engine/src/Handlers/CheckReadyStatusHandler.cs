namespace QueryEngine.Handlers
{
    using System;
    using System.Threading.Tasks;
    using System.Runtime.Serialization.Json;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using QueryEngine.Services;

    public class CheckReadyStatusHandler : BaseHandler<bool, string>
    {
        public CheckReadyStatusHandler(RequestDelegate next) : base(next) { }

        protected override bool Handle(string path)
        {
            return path.Contains("/checkreadystatus");
        }

        protected override bool Execute(string input)
        {
            return true;
        }
    }
}