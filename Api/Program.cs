using Gelf.Extensions.Logging;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateBuildWebHost(args).Build().Run();
        }
        public static IHostBuilder CreateBuildWebHost(string[] args) =>
            // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host?view=aspnetcore-3.1#default-builder-settings
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureLogging((c, b) =>
                {
                    b.AddConsole();

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
