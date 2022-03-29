using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AutocompleteService.MessageBus;
using AutocompleteService.Messges.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using AutocompleteService.Models;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using AutocompleteService.Services;
using System.Net;
using System.Text;
using ApiGateway.Elasticsearch;

namespace AutocompleteService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AutocompleteController : ControllerBase
    {
        private readonly IRankApiService _rankApiService;
        private readonly IIndexApiService _indexApiService;
        private readonly IMessageBusClient _messageBus;

        public AutocompleteController(
            IMessageBusClient messageBus,
            IIndexApiService indexApiService,
            IRankApiService rankApiService)
        {
            _messageBus = messageBus;
            _indexApiService = indexApiService;
            _rankApiService = rankApiService;
        }

        [HttpGet]
        [Produces("application/json")]
        public async Task<IEnumerable<Word>> GetAutocompleteResultAsync([FromQuery] string text) 
        {
            var result = await GetAutocompleteDataAsync(text);

            SendSearchData(result, text);

            return result.Select(index => new Word
            {
                Text = index
            })
            .ToArray();
        }

        private async Task<List<string>> GetAutocompleteDataAsync(string text)
        {
            var transactionId = Guid.NewGuid();
            var sample = new List<string>() { "Your search is incorrect or some exception was thrown. See in logs!" };

            var validatorStatusCode = await TryValidateWordAsync(new ValidatorKeys { Id = transactionId, Keyword = text });

            if (validatorStatusCode == HttpStatusCode.BadRequest)
            {
                ElkSearching.logger.Fatal("Your search is incorrect or some exception was thrown. See in logs!");
                return sample;
            }

            var (searches, rankResponseCode) = await MakeRankRequest(text);

            if (rankResponseCode == HttpStatusCode.BadRequest)
            {
                await TryAbortTransactionValidateWordAsync(transactionId);
                ElkSearching.logger.Fatal("Your search is incorrect or some exception was thrown. See in logs!");
                return sample;
            }

            var (indexes, indexResponseCode) = await MakeIndexRequest(text);

            if (indexResponseCode == HttpStatusCode.BadRequest)
            {
                await TryAbortTransactionValidateWordAsync(transactionId);
                ElkSearching.logger.Fatal("Your search is incorrect or some exception was thrown. See in logs!");
                return sample;
            }


            return indexes
                .Concat(searches)
                .Distinct()
                .ToList();
        }

        private void SendSearchData(List<string> searches, string text)
        {
            var searchesCommand = new SearchesCommand
            {
                Searches = searches,
                Text = text,
            };

            foreach (var @event in searchesCommand.Events)
            {
                var routingKey = "cache-autoCompleteData";

                _messageBus.Publish(
                    message: @event,
                    routingKey: routingKey,
                    exchange: "cache-service"
                );
            }

            ElkSearching.logger.Information($"Search text: {text} -> sent from autocomplete service to Cache service");
        }

        private async Task<HttpStatusCode> TryValidateWordAsync(ValidatorKeys data)
        {
            try
            {
                using var client = new HttpClient();
                client.BaseAddress = new Uri("http://validator:80/");

                var content = new StringContent(JsonConvert.SerializeObject(data).ToString(), Encoding.UTF8, "application/json");
                Console.WriteLine(JsonConvert.SerializeObject(data).ToString());
                HttpResponseMessage response = await client.PostAsync($"Validator/validate", content);

                return response.StatusCode;
            }
            catch (Exception e)
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

        private async Task<(List<string>, HttpStatusCode)> MakeRankRequest(string text)
        {
            try
            {
                HttpResponseMessage responseRank = await _rankApiService.GetRankSearchesAsync(text);

                var searches = JsonConvert
                         .DeserializeObject<List<string>>(
                             await responseRank.Content.ReadAsStringAsync()
                         );

                return (searches, responseRank.StatusCode);
            }
            catch(Exception e)
            {
                ElkSearching.logger.Error(e, "Rank request failed!");
                return (new List<string>(), HttpStatusCode.BadRequest);
            }
        }

        private async Task<(List<string>, HttpStatusCode)> MakeIndexRequest(string text)
        {
            try
            {
                HttpResponseMessage responseIndex = await _indexApiService.GetIndexesAsync(text);

                var indexes = JsonConvert
                        .DeserializeObject<List<string>>(
                            await responseIndex.Content.ReadAsStringAsync()
                        );

                return (indexes, responseIndex.StatusCode);
            }
            catch (Exception e)
            {
                ElkSearching.logger.Error(e, "Index request failed!");
                return (new List<string>(), HttpStatusCode.BadRequest);
            }
        }

        private Task<HttpResponseMessage> MakeRequest(string uri, string path)
        {
            return Task.Run(() =>
            {
                HttpClient httpClient = new HttpClient
                {
                    BaseAddress = new Uri(uri)
                };
                return httpClient.GetAsync(path);
            });
        }

        private async Task<List<string>> DeserializeResponse(HttpResponseMessage response)
        {
            return JsonConvert
                     .DeserializeObject<List<string>>(
                         await response.Content.ReadAsStringAsync()
                     );
        }
    }
}
