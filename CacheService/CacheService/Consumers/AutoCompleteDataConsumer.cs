using CacheService.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace CacheService.Consumers
{
    public class AutoCompleteDataConsumer :
        AbstractConsumer<AutoCompleteData>
    {
        public AutoCompleteDataConsumer(
            IServiceProvider serviceProvider
        ) : base(serviceProvider)
        {
            Queue = "cache-service/autoCompleteData";

            _routingKey = "cache-autoCompleteData";

            _exchange = "cache-service";

            InitializeEventBus();
        }

        protected override void LogMessageReceived(AutoCompleteData searches)
        {
            Console.WriteLine(
                $"Message Cache service received"
            );
        }

        protected override async Task RecieveSearchData(AutoCompleteData searches)
        {
            if(searches.Text != null)
            {
                var scope = _serviceProvider.CreateScope();
                var cacheService = scope.ServiceProvider.GetService<IDistributedCache>();
                await Extensions.DistributedCacheExtensions.SetRecordAsync(cacheService, searches.Text, searches.Searches);
            }
        }
    }
}
