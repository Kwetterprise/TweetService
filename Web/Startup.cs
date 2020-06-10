using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Web
{
    using Business;
    using Business.EventProcessor;
    using Business.Manager;
    using Confluent.Kafka;
    using Data.Context;
    using Kwetterprise.EventSourcing.Client.Interface;
    using Kwetterprise.EventSourcing.Client.Kafka;
    using Kwetterprise.EventSourcing.Client.Local;
    using Kwetterprise.EventSourcing.Client.Models.Event;
    using Kwetterprise.ServiceDiscovery.Client;
    using Kwetterprise.ServiceDiscovery.Client.Middleware;
    using Kwetterprise.ServiceDiscovery.Client.Models;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Diagnostics;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Server.Kestrel.Core;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.IdentityModel.Tokens;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Configure url.
            services.Configure<KestrelServerOptions>(this.Configuration.GetSection("Kestrel"));

            Startup.RegisterDatabase(services);
            Startup.ConfigureCustomServices(services);
            Startup.RegisterKafkaServices(services, this.Configuration);
            Startup.RegisterDiscoveryClient(services, this.Configuration);
            Startup.ConfigureAuthentication(services, this.Configuration);
            Startup.RegisterEventProcessor(services);

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(configure =>
                {
                    configure.Run(async context =>
                    {
                        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();

                        switch (exceptionHandlerPathFeature)
                        {
                            default:
                                {
                                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                                    await context.Response.WriteAsync(exceptionHandlerPathFeature.ToString());
                                    break;
                                }
                        }
                    });
                });
            }

            app.UseHttpsRedirection();

            app.AddServiceDiscoveryPingMiddleware();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private static void RegisterEventProcessor(IServiceCollection services)
        {
            services.AddSingleton<ITweetEventProcessor, TweetEventProcessor>();
            services.AddHostedService<EventProcessorService>();
        }

        private static void ConfigureAuthentication(IServiceCollection services, IConfiguration configuration)
        {
            var jwtSection = configuration.GetSection("Jwt");
            var jwtConfiguration = new JwtConfiguration(jwtSection["Issuer"], jwtSection["Key"]);
            services.AddSingleton(jwtConfiguration);

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = true;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = new SymmetricSecurityKey(jwtConfiguration.Key),
                    ValidIssuer = jwtConfiguration.Issuer,
                    ValidAudience = jwtConfiguration.Issuer,
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                };
            });

            services.AddMvc();
        }

        private static void ConfigureCustomServices(IServiceCollection services)
        {
            services.AddTransient<ITweetCommandManager, TweetCommandManager>();
            services.AddTransient<ITweetQueryManager, TweetQueryManager>();
        }

        private static void RegisterDiscoveryClient(IServiceCollection services, IConfiguration configuration)
        {
            var serviceSection = configuration.GetSection("ServiceDiscovery");

            var serviceConfiguration = new ServiceConfiguration(serviceSection["ServiceName"], serviceSection["ServiceUrl"]);
            var apiGatewayConfiguration = new ServiceDiscoveryConfiguration(serviceSection["ServiceDiscoveryUrl"]);
            services.AddServiceDiscoveryClientWorker(serviceConfiguration, apiGatewayConfiguration);
        }

        private static void RegisterKafkaServices(IServiceCollection services, IConfiguration configuration)
        {
            var (kafkaConfiguration, kafkaConsumerConfiguration) = Startup.CreateKafkaConfigurations(configuration);

            services.AddSingleton(kafkaConfiguration);
            services.AddSingleton(kafkaConsumerConfiguration);

            //services.AddTransient<IEventPublisher, KafkaEventPublisher>();
            //services.AddTransient<IEventListener, KafkaEventListener>();

            var eventManager = new LocalEventManager();

            services.AddSingleton<IEventPublisher>(eventManager);
            services.AddSingleton<IEventListener>(eventManager);
        }

        private static (KafkaConfiguration, KafkaConsumerConfiguration) CreateKafkaConfigurations(IConfiguration configuration)
        {
            var kafkaSection = configuration.GetSection("Kafka");
            return (new KafkaConfiguration(kafkaSection["Servers"]),
                new KafkaConsumerConfiguration(
                    kafkaSection["Servers"],
                    kafkaSection.GetValue<IEnumerable<Topic>>("Topics"),
                    kafkaSection["GroupId"],
                    kafkaSection.GetValue<AutoOffsetReset>("Offset")));
        }

        private static void RegisterDatabase(IServiceCollection services)
        {
            var optionsBuilder = new DbContextOptionsBuilder();

            static DbContextOptionsBuilder BuildOptions(DbContextOptionsBuilder optionsBuilder)
            {
                return optionsBuilder.UseInMemoryDatabase("Tweet");
            }

            var options = BuildOptions(optionsBuilder).Options;
            // services.AddDbContext<AccountContext>(o => BuildOptions(o));

            services.AddSingleton<ITweetContextFactory, TweetContextFactory>(_ => new TweetContextFactory(options));
        }
    }
}
