using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Kwetterprise.TweetService.Business;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;

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

            services.AddSwaggerGen(c =>
            {
                //c.GeneratePolymorphicSchemas(infoType => {
                //    if (infoType == typeof(Option<>))
                //    {
                //        return new Type[] {
                //            typeof(Option<AccountDto>),
                //            typeof(Option<AccountWithTokenDto>)
                //        };
                //    }
                //    return Enumerable.Empty<Type>();
                //}, discriminator => discriminator == typeof(Option<>) ? "dataType" : null);
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "AccountService", Version = "v1" });

                // c.ExampleFilters();

                // c.OperationFilter<AddHeaderOperationFilter>("correlationId", "Correlation Id for the request", false); // adds any string you like to the request headers - in this case, a correlation id
                c.OperationFilter<AddResponseHeadersFilter>(); // [SwaggerResponseHeader]

                c.OperationFilter<AppendAuthorizeToSummaryOperationFilter>(); // Adds "(Auth)" to the summary so that you can see which endpoints have Authorization
                // or use the generic method, e.g. c.OperationFilter<AppendAuthorizeToSummaryOperationFilter<MyCustomAttribute>>();

                // add Security information to each operation for OAuth2
                c.OperationFilter<SecurityRequirementsOperationFilter>();
                // or use the generic method, e.g. c.OperationFilter<SecurityRequirementsOperationFilter<MyCustomAttribute>>();


                c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Description = "Standard Authorization header using the Bearer scheme. Example: \"bearer {token}\"",
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                });

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "TweetService API");
                c.RoutePrefix = string.Empty;
            });

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

            services.AddSingleton<JwtManager>();

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
            var kafkaConsumerConfiguration = Startup.CreateKafkaConfigurations(configuration);

            services.AddSingleton<KafkaConfiguration>(kafkaConsumerConfiguration);
            services.AddSingleton(kafkaConsumerConfiguration);

            services.AddTransient<IEventPublisher, KafkaEventPublisher>();
            services.AddTransient<IEventListener, KafkaEventListener>();
        }

        private static KafkaConsumerConfiguration CreateKafkaConfigurations(IConfiguration configuration)
        {
            var kafkaSection = configuration.GetSection("Kafka");

            var topicNames = kafkaSection.GetSection("Topics").GetChildren().Select(x => x.Get<string>());

            return new KafkaConsumerConfiguration(
                kafkaSection["Servers"],
                topicNames.Select(x => new Topic(x)),
                kafkaSection["GroupId"] + $"_{Guid.NewGuid()}",
                kafkaSection.GetValue<AutoOffsetReset>("Offset"));
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
