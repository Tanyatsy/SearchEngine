using CacheService.Elasticsearch;
using CacheService.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
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
            ElkSearching.logger.Information($"Message Cache service received: {JsonConvert.SerializeObject(searches)}");
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
