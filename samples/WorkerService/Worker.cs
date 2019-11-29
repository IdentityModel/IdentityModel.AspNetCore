using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly HttpClient _client;

        public Worker(ILogger<Worker> logger, IHttpClientFactory factory)
        {
            _logger = logger;
            _client = factory.CreateClient("client");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("\n\nWorker running at: {time}", DateTimeOffset.Now);

                var response = await _client.GetStringAsync("test");
                Console.WriteLine(response);

                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}
