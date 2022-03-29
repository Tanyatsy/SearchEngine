using ValidatorService.Consumers;
using ValidatorService.Context;
using ValidatorService.MessageBus;
using ValidatorService.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

namespace ValidatorService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "ValidatorService", Version = "v1" });
            });

            services.AddHostedService<WordConsumer>();
            services.AddScoped<IMessageBusClient, RabbitMQClient>();

            var connectionString = Configuration["DbContextSettings:ConnectionString"];
            services.AddDbContext<PageDataContext>(
                opts => opts.UseNpgsql(connectionString)
            );
            services.Configure<IValidatorDatabaseSettings>(
               Configuration.GetSection(nameof(ValidatorDatabaseSettings)));

            services.AddSingleton<IValidatorDatabaseSettings>(sp =>
                sp.GetRequiredService<IOptions<ValidatorDatabaseSettings>>().Value);

            services.AddTransient<ValidatorRepository>();

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ValidatorService v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
