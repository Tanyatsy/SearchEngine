using SearchService.Models;
using Serilog;
using Serilog.Core;
using Serilog.Sinks.Elasticsearch;
using System;

namespace SearchService.Elasticsearch
{
    public class ElkSearching
    {
        public static LoggerConfiguration loggerConfig = new LoggerConfiguration()
                .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://elasticsearch:9200"))
                {
                    IndexFormat = "search-service-{0:yyyy.MM.dd}",
                    AutoRegisterTemplate = true,
                    AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv6
                });
        public static Logger logger = loggerConfig.CreateLogger();

        public static void SaveToElkWord(Word word)
        {
            // var json = "{\"name\": \"" + word.Text + "\"}";
          /*  using var client = new HttpClient();
            client.BaseAddress = new Uri("http://elasticsearch:9200/");*/

            // HttpResponseMessage response = await client.PutAsync($"searchengine/word/" + Guid.NewGuid(), new StringContent(json, Encoding.UTF8, "application/json"));
        }

    }
}
