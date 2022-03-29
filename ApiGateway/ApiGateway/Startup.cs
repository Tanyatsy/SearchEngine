using ApiGateway.Aggregators;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ocelot.Cache.CacheManager;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Polly;
using Polly;
using System;
using System.Net.Http;

namespace ApiGateway
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOcelot().AddPolly()
                .AddCacheManager(x => x.WithDictionaryHandle())
                .AddTransientDefinedAggregator<CacheAggregator>()
                .AddTransientDefinedAggregator<CacheCompleteAggregator>();

            var basicCircuitBreakerPolicy = Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .CircuitBreakerAsync(10, TimeSpan.FromSeconds(60), OnBreak, OnReset, OnHalfOpen);

            services.AddHttpClient("SearchService", client =>
            {
                client.BaseAddress = new Uri("http://search:80/");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            }).AddPolicyHandler(basicCircuitBreakerPolicy);

            services.AddHttpClient("AutocompleteService", client =>
            {
                client.BaseAddress = new Uri("http://autocomplete:80/");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            }).AddPolicyHandler(basicCircuitBreakerPolicy);


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
            });

            app.UseOcelot().Wait();
        }

        private void OnHalfOpen()
        {
            Console.WriteLine("Circuit in test mode, one request will be allowed.");
        }

        private void OnReset()
        {
            Console.WriteLine("Circuit closed, requests flow normally.");
        }

        private void OnBreak(DelegateResult<HttpResponseMessage> result, TimeSpan ts)
        {
            Console.WriteLine("Circuit cut, requests will not flow.");
        }
    }
}
