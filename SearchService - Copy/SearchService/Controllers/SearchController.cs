using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SearchService.MessageBus;
using SearchService.Messges.Commands;
using SearchService.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SearchService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly IMessageBusClient _messageBus;
        private readonly ILogger<SearchController> _logger;

        public SearchController(
            ILogger<SearchController> logger,
            IMessageBusClient messageBus)
        {
            _messageBus = messageBus;
            _logger = logger;
        }

        [HttpGet]
        public async Task<List<PageData>> GetSearchResultAsync([FromQuery] string text)
        {
            SendSearchText(text);

            var pageData = await GetPageDataAsync(text);

            SendSearchData(pageData, text);

            return pageData;
        }
/*
        [HttpGet]
        public async Task<IActionResult> GetSearchResultAsync([FromQuery] string text)
        {
            SendSearchText(text);

            var pageData = await GetPageDataAsync(text);

            SendSearchData(pageData, text);

            return StatusCode(500);
        }
*/
        private async Task<List<PageData>> GetPageDataAsync(string text)
        {
            using var client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:5001/");
            client.Timeout = TimeSpan.FromMinutes(1);


            HttpResponseMessage response = await client.GetAsync($"Index/pagedata/{text}");

            return JsonConvert
                    .DeserializeObject<List<PageData>>(
                        await response.Content.ReadAsStringAsync()
                    );
        }

        private void SendSearchText(string text)
        {
            var wordCommand = new WordCommand
            {
                Word = text
            };

            foreach (var @event in wordCommand.Events)
            {
                var routingKey = "rank-searchWord";

                _messageBus.Publish(
                    message: @event,
                    routingKey: routingKey,
                    exchange: "rank-service"
                );
            }
        }

        private void SendSearchData(List<PageData> data, string text)
        {
            var searchDataCommand = new SearchDataCommand
            {
                PageData = data,
                SearchText = text,
            };

            foreach (var @event in searchDataCommand.Events)
            {
                var routingKey = "cache-searchData";

                _messageBus.Publish(
                    message: @event,
                    routingKey: routingKey,
                    exchange: "cache-service"
                );
            }
        }
    }

    public class TestHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                return await base.SendAsync(request, cancellationToken);
            }
            catch (System.Exception ex)
            {
                _ = ex;
                throw;
            }
        }
    }
}
