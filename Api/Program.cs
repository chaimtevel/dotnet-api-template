using Gelf.Extensions.Logging;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateBuildWebHost(args).Build().Run();
        }

        public static IWebHostBuilder CreateBuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .ConfigureLogging((c, b) =>
                {
                    b.Services.Configure<GelfLoggerOptions>(c.Configuration.GetSection("Graylog"));

                    b.AddConfiguration(c.Configuration.GetSection("Logging"))
                        .AddConsole();

                    bool debugGraylog = c.Configuration.GetValue<bool>("Logging:GELF:DebugGraylog", false);
                    if (!c.HostingEnvironment.IsDevelopment() || debugGraylog)
                    {
                        b.AddGelf(o =>
                        {
                            o.LogSource = System.Environment.MachineName;
                            o.AdditionalFields.Add("environment", c.HostingEnvironment.EnvironmentName);
                        });
                    }

                });
    }
}
