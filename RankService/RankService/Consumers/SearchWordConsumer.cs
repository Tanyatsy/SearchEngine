using Microsoft.Extensions.DependencyInjection;
using RankService.Models;
using RankService.Repositories;
using System;
using System.Threading.Tasks;

namespace RankService.Consumers
{
    public class SearchWordConsumer :
        AbstractConsumer<SearchData>
    {
        public SearchWordConsumer(
             IServiceProvider serviceProvider
        ) : base(serviceProvider)
        {
            Queue = "rank-service/searchWord";

            _routingKey = "rank-searchWord";

            _exchange = "rank-service";

            InitializeEventBus();
        }

        protected override void LogMessageReceived(
            SearchData message
        )
        {
            Console.WriteLine(
                $"Message received with Text {message.Text}"
            );
        }

        protected override async Task RecieveSearchData(SearchData data)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var searchDataService = scope.ServiceProvider.GetService<ISearchDataService>();
                if(searchDataService.FindByWord(data.Text).Count == 0) 
                { 
                    searchDataService.Create(data);
                }
            }
        
            await Task.Delay(1);
        }
    }
}
