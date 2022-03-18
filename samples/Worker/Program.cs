using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using IdentityModel.Client;
using Serilog.Sinks.SystemConsole.Themes;
using IdentityModel.AspNetCore.AccessTokenManagement.Azure;
using Azure.Identity;

namespace WorkerService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                                .MinimumLevel.Debug()
                                .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                                .CreateLogger();

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            const string vaultPrefix = "AzureKeyVault";

            var host = Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddClientAccessTokenManagement(options =>
                    {
                        options.Clients.Add("identityserver", new ClientCredentialsTokenRequest
                        {
                            Address = "https://demo.duendesoftware.com/connect/token",
                            ClientId = "m2m.short",
                            ClientSecret = "secret",
                            Scope = "api"
                        });
                    })
                    .WithAzureKeyVault(opts =>
                    {
                        opts.Url = new Uri(hostContext.Configuration[$"{vaultPrefix}:Url"]);

                        opts.Credential = new ClientSecretCredential(
                            hostContext.Configuration[$"{vaultPrefix}:TenantId"],
                            hostContext.Configuration[$"{vaultPrefix}:ClientId"],
                            hostContext.Configuration[$"{vaultPrefix}:ClientSecret"]);          
                    });

                    services.AddClientAccessTokenHttpClient("client", configureClient: client =>
                    {
                        client.BaseAddress = new Uri("https://demo.duendesoftware.com/api/");
                    });

                    services.AddHostedService<Worker>();
                });

            return host;
        }
            
    }
}
