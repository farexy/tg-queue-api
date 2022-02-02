using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TG.Core.App.Configuration;
using TG.Core.App.Configuration.TgConfig;
using TG.Queue.Api.Config;

namespace TG.Queue.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureTgKeyVault()
                .ConfigureAppConfiguration((ctx, configuration) =>
                {
                    configuration.AddTgConfigs(TgConfigs.BattleSettings);
                })
                .ConfigureLogging(logging => logging.AddSimpleConsole(c =>
                    {
                        c.TimestampFormat = "[yyyy-MM-dd HH:mm:ss] ";
                        c.UseUtcTimestamp = true;
                    }
                ))
                //.ConfigureTgLogging(ServiceConst.ServiceName)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}