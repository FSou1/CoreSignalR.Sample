using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SignalR.Core.Sample
{
    public class CurrencyListenerService : IHostedService, IDisposable
    {
        private readonly HubLifetimeManager<CurrencyHub> _hubManager;
        private readonly ILogger<CurrencyListenerService> _logger;
        private readonly TimeSpan _delay = TimeSpan.FromSeconds(0.5);
        private readonly Random _rnd = new Random();

        public CurrencyListenerService(
            HubLifetimeManager<CurrencyHub> hubManager,
            ILogger<CurrencyListenerService> logger)
        {
            _hubManager = hubManager;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cts)
        {
            _logger.LogInformation("Currency listener is starting..");

            while (!cts.IsCancellationRequested)
            {
                _logger.LogInformation("Fetch currency updates..");

                var currencies = await FetchCurrencyUpdates();

                await Broadcast(currencies);

                await Task.Delay(_delay, cts);
            }

            _logger.LogInformation("Currency listener has completed..");
        }

        private Task<Dictionary<string, double>> FetchCurrencyUpdates()
        {
            var data = new Dictionary<string, double>
            {
                {"EUR", 80 + _rnd.NextDouble()},
                {"USD", 57 + _rnd.NextDouble()},
                {"BYN", 28 + _rnd.NextDouble()}
            };

            return Task.FromResult(data);
        }

        private async Task Broadcast(Dictionary<string, double> data)
        {
            await _hubManager.InvokeAllAsync("currenciesUpdated", 
                new object[] { data });
        }

        public Task StopAsync(CancellationToken cts)
        {
            _logger.LogInformation("Currency listener is stopping..");

            return Task.CompletedTask;
        }

        public void Dispose()
        {
        }
    }
}