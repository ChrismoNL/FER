using Chrismo.FER.Models;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Chrismo.FER
{
    public class RatesClient
    {
        private const string baseUrl = "https://api.exchangeratesapi.io";
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _memoryCache;

        public RatesClient(HttpClient httpClient, IMemoryCache memoryCache)
        {
            _httpClient = httpClient;
            _memoryCache = memoryCache;
        }

        public Task<Dictionary<Currencies, decimal>> GetLatestAsync()
        {
            return GetLatestAsync(Currencies.EUR);
        }

        public async Task<Dictionary<Currencies, decimal>> GetLatestAsync(Currencies baseCurrency)
        {
            string cacheName = $"FER-RATES-{baseCurrency}";
            if (_memoryCache.TryGetValue(cacheName, out Dictionary<Currencies, decimal> rates))
            {
                return rates;
            }

            var result = await _httpClient.GetAsync($"{baseUrl}/latest?base={baseCurrency}").ConfigureAwait(false);
            if (result.IsSuccessStatusCode)
            {
                string rawContent = await result.Content.ReadAsStringAsync().ConfigureAwait(false);

                RatesResponse ratesResponse = JsonConvert.DeserializeObject<RatesResponse>(rawContent);
                if(ratesResponse != null && ratesResponse.Rates.Any())
                {
                    return _memoryCache.Set(cacheName, ratesResponse.Rates, GetNextRefreshDateTime(ratesResponse.Date).Subtract(DateTime.UtcNow.AddHours(1)));
                }
            }
            return new Dictionary<Currencies, decimal>();
        }

        /// <summary>
        /// Calculate the next currency refresh date. This is the next working day at 16:00pm CET.
        /// Ignoring TARGET closing days as they are to complicated for now.
        /// </summary>
        /// <param name="lastRefreshDate">The last time the rates are refreshed</param>
        /// <returns>The datetime when the rates are refreshed.</returns>
        private DateTime GetNextRefreshDateTime(DateTime lastRefreshDate)
        {
            var nextDate = lastRefreshDate.AddDays(1);
            while(IsWeekend(nextDate))
            {
                nextDate = nextDate.AddDays(1);
            }

            return nextDate.AddHours(16).AddMinutes(15);
        }

        /// <summary>
        /// Check if date is in the weekend
        /// </summary>
        /// <param name="date">The date to check</param>
        /// <returns>True if it is in the weekend and False if it is not</returns>
        private static bool IsWeekend(DateTime date)
        {
            return date.DayOfWeek == DayOfWeek.Saturday
                || date.DayOfWeek == DayOfWeek.Sunday;
        }
    }
}
