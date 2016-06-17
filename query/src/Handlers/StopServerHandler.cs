namespace QueryEngine.Handlers
{
    using Microsoft.AspNetCore.Http;

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