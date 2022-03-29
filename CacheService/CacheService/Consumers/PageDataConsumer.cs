using CacheService.Elasticsearch;
using CacheService.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace CacheService.Consumers
{
    public class PageDataConsumer :
        AbstractConsumer<SearchData>
    {
        public PageDataConsumer(
            IServiceProvider serviceProvider
        ) : base(serviceProvider)
        {
            Queue = "cache-service/searchData";

            _routingKey = "cache-searchData";

            _exchange = "cache-service";

            InitializeEventBus();
        }

        protected override void LogMessageReceived(SearchData data)
        {
           ElkSearching.logger.Information($"Cache received message with text: {data.SearchText}");
        }

        protected override async Task RecieveSearchData(SearchData data)
        {
            var scope = _serviceProvider.CreateScope();
            var cacheService = scope.ServiceProvider.GetService<IDistributedCache>();
            await Extensions.DistributedCacheExtensions.SetRecordAsync(cacheService, data.SearchText, data.PageData);
        }
    }
}
