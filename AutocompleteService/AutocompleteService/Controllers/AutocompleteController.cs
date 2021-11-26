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

namespace AutocompleteService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AutocompleteController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Coffee", "Coffee Shop", "Coffee Barista", "Coffee Expert", "Coffee Rating"
        };

        private readonly IRankApiService _rankApiService;
        private readonly IIndexApiService _indexApiService;
        private readonly IMessageBusClient _messageBus;
        private readonly ILogger<AutocompleteController> _logger;

        public AutocompleteController(
            ILogger<AutocompleteController> logger,
            IMessageBusClient messageBus,
            IIndexApiService indexApiService,
            IRankApiService rankApiService)
        {
            _messageBus = messageBus;
            _logger = logger;
            _indexApiService = indexApiService;
            _rankApiService = rankApiService;
        }

        [HttpGet]
        public async Task<IEnumerable<Word>> GetAutocompleteResultAsync([FromQuery] string text) 
        {
            var wordCommand = new WordCommand
            {
                Word = text
            };

            foreach (var @event in wordCommand.Events)
            {
                var routingKey = "index-word";

                _messageBus.Publish(
                    message: @event,
                    routingKey: routingKey,
                    exchange: "index-service"
                );
            }

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
            
            HttpResponseMessage responseRank = await _rankApiService.GetRankSearchesAsync(text);
            HttpResponseMessage responseIndex = await _indexApiService.GetIndexesAsync(text);

            var indexes = JsonConvert
                    .DeserializeObject<List<string>>(
                        await responseIndex.Content.ReadAsStringAsync()
                    );

            var searches = JsonConvert
                     .DeserializeObject<List<string>>(
                         await responseRank.Content.ReadAsStringAsync()
                     );


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
