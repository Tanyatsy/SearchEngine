using Serilog;
using Serilog.Core;
using Serilog.Sinks.Elasticsearch;
using System;

namespace AutocompleteServiceS.Elasticsearch
{
    public class ElkSearching
    {
        public static LoggerConfiguration loggerConfig = new LoggerConfiguration()
                .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://elasticsearch:9200"))
                {
                    IndexFormat = "autocomplete-service-{0:yyyy.MM.dd}",
                    AutoRegisterTemplate = true,
                    AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv6
                });

        public static Logger logger = loggerConfig.CreateLogger();
    }
}
