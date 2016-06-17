namespace QueryEngine
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.AspNetCore.Hosting;
    using System.Globalization;

    public class Program
    {
        public static IApplicationLifetime AppLifeTime;

        public static void Main(string[] args)
        {
            // todo
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            CultureInfo.CurrentUICulture = new CultureInfo("en-US");
            var config = new ConfigurationBuilder()
                .AddCommandLine(new[] { "--server.urls", "http://localhost:8111" });
            var builder = new WebHostBuilder()
                .UseConfiguration(config.Build())
                .UseStartup(typeof(Startup))
                .UseServer("Microsoft.AspNetCore.Server.Kestrel");

            using (var app = builder.Build())
            {
                app.Start();
                AppLifeTime = (IApplicationLifetime) app.Services.GetService(typeof(IApplicationLifetime));
                AppLifeTime.ApplicationStopping.WaitHandle.WaitOne();
            }
        }
    }
}
