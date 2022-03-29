using SearchService.Elasticsearch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SearchService.Elasticsearch;
using SearchService.MessageBus;
using SearchService.Messges.Commands;
using SearchService.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
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

        [HttpGet("/hello")]
        public string GetSearchResult()
        {
            return "hello";
        }

        [HttpGet]
        [Produces("application/json")]
        public async Task<List<PageData>> GetSearchResultAsync([FromQuery] string text)
        {
            SendSearchText(text);

            var pageData = await GetPageDataAsync(text);
       
            ElkSearching.logger.Information($"Idex_service response: {JsonConvert.SerializeObject(pageData)}");
         
            SendSearchData(pageData, text);

            return pageData;
        }

        private async Task<List<PageData>> GetPageDataAsync(string text)
        {
            var transactionId = Guid.NewGuid();
            var sample = new List<PageData>() { new PageData() { Id = transactionId, Text = "Your word is incorrect or some exception was thrown. See in logs!" } };
            var validatorStatusCode = await TryValidateWordAsync(new ValidatorKeys { Id = transactionId, Keyword = text });

            if(validatorStatusCode == HttpStatusCode.BadRequest)
            {
                ElkSearching.logger.Fatal("Your word is incorrect or some exception was thrown. See in logs!");
                return sample;
            }

            var (searchResponse, searchStatusCode)  = await TryGetPageDataByValidWordAsync(text);

            if (searchStatusCode ==  HttpStatusCode.BadRequest) 
            {
                await TryAbortTransactionValidateWordAsync(transactionId);
                ElkSearching.logger.Fatal("Your word is incorrect or some exception was thrown. See in logs!");
                return sample;

            }

            return searchResponse;
        }

        private async Task<(List<PageData>, HttpStatusCode)> TryGetPageDataByValidWordAsync(string word)
        {
            try
            {
                using var client = new HttpClient();
                client.BaseAddress = new Uri("http://index:80/");

                HttpResponseMessage response = await client.GetAsync($"Index/pagedata/{word}");
                var pageData = JsonConvert
                        .DeserializeObject<List<PageData>>(
                            await response.Content.ReadAsStringAsync());
                return (pageData, HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                ElkSearching.logger.Error(e, "Index request failed!");
                return (new List<PageData>(), HttpStatusCode.BadRequest);
            }
        }

        private async Task<HttpStatusCode> TryValidateWordAsync(ValidatorKeys data)
        {
            try
            {
                using var client = new HttpClient();
                client.BaseAddress = new Uri("http://validator:80/");

                var content = new StringContent(JsonConvert.SerializeObject(data).ToString(), Encoding.UTF8, "application/json");
              
                HttpResponseMessage response = await client.PostAsync($"Validator/validate", content);

                return response.StatusCode;
            }
            catch(Exception e)
            {
                ElkSearching.logger.Error(e, "Validator request failed!");
                return HttpStatusCode.BadRequest;
            }
        }

        private async Task<HttpStatusCode> TryAbortTransactionValidateWordAsync(Guid transactionId)
        {
            try
            {
                using var client = new HttpClient();
                client.BaseAddress = new Uri("http://validator:80/");

                HttpResponseMessage response = await client.DeleteAsync($"Validator/abort/{transactionId}");

                return response.StatusCode;
            }
            catch (Exception e)
            {
                ElkSearching.logger.Error(e, "Validator abort transaction request failed!");
                return HttpStatusCode.BadRequest;
            }
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

            ElkSearching.logger.Information($"Search text: {text} -> sent from search service to Rank service");
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

            ElkSearching.logger.Information($"Search text: {text} -> sent from search service to Cache service");
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

    public class ExampleHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var healthCheckResultHealthy = true;

            if (healthCheckResultHealthy)
            {
                return Task.FromResult(
                    HealthCheckResult.Healthy("A healthy result."));
            }

            return Task.FromResult(
                new HealthCheckResult(context.Registration.FailureStatus,
                "An unhealthy result."));
        }
    }
}
