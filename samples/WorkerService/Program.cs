using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;

namespace WorkerService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                                .MinimumLevel.Debug()
                                .WriteTo.Console()
                                .CreateLogger();

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddAccessTokenManagement(options =>
                    {
                        options.Client.Clients.Add("identityserver", new IdentityModel.Client.TokenClientOptions
                        {
                            Address = "https://demo.identityserver.io/connect/token",
                            ClientId = "m2m.short",
                            ClientSecret = "secret"
                        });
                    });

                    services.AddClientAccessTokenClient("client", client =>
                    {
                        client.BaseAddress = new Uri("https://demo.identityserver.io/api/");
                    });

                    services.AddHostedService<Worker>();
                });
    }
}
