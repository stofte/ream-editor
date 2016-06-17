namespace QueryEngine.Handlers
{
    using Microsoft.AspNetCore.Http;

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