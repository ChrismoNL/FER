using Chrismo.FER.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Chrismo.FER.Tests.IntegrationTests
{
    public class RatesClientTests
    {
        [Fact]
        public async Task GetAllRates()
        {
            var services = new ServiceCollection();
            services.AddMemoryCache();
            var serviceProvider = services.BuildServiceProvider();
            var memoryCache = serviceProvider.GetService<IMemoryCache>();

            using(HttpClient httpClient = new HttpClient())
            {
                RatesClient ratesClient = new RatesClient(httpClient, memoryCache);
                Dictionary<Currencies, decimal> currencyValues = await ratesClient.GetLatestAsync().ConfigureAwait(false);
                Assert.True(currencyValues.Any());
            }
        }
    }
}
