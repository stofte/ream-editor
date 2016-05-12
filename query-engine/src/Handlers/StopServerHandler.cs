namespace QueryEngine.Handlers
{
    using System;
    using System.Threading.Tasks;
    using System.Runtime.Serialization.Json;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using QueryEngine.Services;

    public class StopServerHandler : BaseHandler<bool, string>
    {
        public StopServerHandler(RequestDelegate next) : base(next) { }

        protected override bool Handle(string path)
        {
            return path.Contains("/stopserver");
        }

        protected override bool Execute(string input)
        {
            QueryEngine.Program.AppLifeTime.StopApplication();
            return true;
        }
    }
}