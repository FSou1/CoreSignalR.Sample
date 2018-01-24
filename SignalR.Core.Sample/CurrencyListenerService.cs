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
        private readonly TimeSpan _delay = TimeSpan.FromSeconds(1);
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
                {"EUR", Get(69) },
                {"USD", Get(56) },
                {"GBR", Get(79) },
                {"INR", Get(1) },
                {"CAD", Get(45) },
                {"MAD", Get(6) },
                {"AUD", Get(45) },
                {"TRY", Get(15) },
                {"AZN", Get(33) },
            };

            return Task.FromResult(data);
        }

        internal double Get(double @default) => Round(@default + _rnd.NextDouble(), 2);

        internal double Round(double val, int digit) => Math.Round(val, digit);

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