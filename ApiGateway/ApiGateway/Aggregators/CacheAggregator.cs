using ApiGateway.Elasticsearch;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocelot.Middleware;
using Ocelot.Multiplexer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ApiGateway.Aggregators
{
    public class CacheAggregator : IDefinedAggregator
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<CacheAggregator> _logger;

        public CacheAggregator(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        public async Task<DownstreamResponse> Aggregate(List<HttpContext> responses)
        {

            List<Header> header = new List<Header>();
            int count = 0;
            try
            {
                var headers = responses.SelectMany(x => x.Items.DownstreamResponse().Headers).ToList();
                var word = responses[0].Request.Path.Value.Split('/').Last();
                var responseCache = await responses[0].Items.DownstreamResponse().Content.ReadAsStringAsync();
                if (responseCache == "[]")
                {
                    var httpClient = _httpClientFactory.CreateClient("SearchService");
                    HttpResponseMessage response = await httpClient.GetAsync($"Search?text={word}");

                    while (!response.IsSuccessStatusCode && count < 2)
                    {
                        await Task.Delay(5000);
                        count++;
                        ElkSearching.logger.Information($"Api makes request on search service with word: {word}");
                       response = await httpClient.GetAsync($"Search?text={word}");
                    }

                    var content = await response.Content.ReadAsStringAsync();

                    return new DownstreamResponse(new StringContent(content, Encoding.UTF8, "application/json"), HttpStatusCode.OK, headers, "OK");
                }
                else
                {
                    return new DownstreamResponse(new StringContent(responseCache, Encoding.UTF8, "application/json"), HttpStatusCode.OK, headers, "OK");
                }
            }
            catch (Exception e)
            {
                ElkSearching.logger.Error(e.Message, "Error in Api Gateway requests for search service");
                return new DownstreamResponse(null, System.Net.HttpStatusCode.InternalServerError, header, null);
            }
        }
    }
}
