using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IHttpClientFactory _clientFactory;

        public Worker(ILogger<Worker> logger, IHttpClientFactory factory)
        {
            _logger = logger;
            _clientFactory = factory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(2000, stoppingToken);
            
            while (!stoppingToken.IsCancellationRequested)
            {
                Console.WriteLine("\n\n");
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                var client = _clientFactory.CreateClient("client");
                var response = await client.GetAsync("test", stoppingToken);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(stoppingToken);
                    _logger.LogInformation("API response: {response}", content);    
                }
                else
                {
                    _logger.LogError("API returned: {statusCode}", response.StatusCode);
                }

                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}