using System.Security.Cryptography.X509Certificates;
using Autofac;
using ChargeBee.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using Ranger.Common;
using Ranger.InternalHttpClient;
using Ranger.RabbitMQ;
using Ranger.Services.Subscriptions.Data;

namespace Ranger.Services.Subscriptions
{
    public class Startup
    {
        private readonly IWebHostEnvironment Environment;
        private readonly IConfiguration configuration;
        private ILoggerFactory loggerFactory;
        private IBusSubscriber busSubscriber;

        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            this.Environment = environment;
            this.configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(options =>
                {
                    options.EnableEndpointRouting = false;
                })
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("subscriptionApi", policyBuilder =>
                    {
                        policyBuilder.RequireScope("subscriptionApi");
                    });
            });

            services.AddSingleton<ITenantsClient, TenantsClient>(provider =>
            {
                return new TenantsClient("http://tenants:8082", loggerFactory.CreateLogger<TenantsClient>());
            });

            services.AddEntityFrameworkNpgsql().AddDbContext<SubscriptionsDbContext>(options =>
            {
                options.UseNpgsql(configuration["cloudSql:ConnectionString"]);
            },
                ServiceLifetime.Transient
            );

            services.AddTransient<ISubscriptionsDbContextInitializer, SubscriptionsDbContextInitializer>();

            services.AddAuthentication("Bearer")
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = "http://identity:5000/auth";
                    options.ApiName = "subscriptionsApi";

                    options.RequireHttpsMetadata = false;
                });

            services.AddDataProtection()
                .ProtectKeysWithCertificate(new X509Certificate2(configuration["DataProtectionCertPath:Path"]))
                .PersistKeysToDbContext<SubscriptionsDbContext>();

            services.AddTransient<SubscriptionsRepository>();
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterInstance<ChargeBeeOptions>(configuration.GetOptions<ChargeBeeOptions>("chargeBee"));
            builder.AddRabbitMq();
        }

        public void Configure(IApplicationBuilder app, IHostApplicationLifetime applicationLifetime, ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;
            applicationLifetime.ApplicationStopping.Register(OnShutdown);
            ApiConfig.Configure(configuration.GetOptions<ChargeBeeOptions>("chargeBee").Site, configuration.GetOptions<ChargeBeeOptions>("chargeBee").ApiKey);

            app.UseRouting();
            app.UseAuthentication();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            this.busSubscriber = app.UseRabbitMQ()
                .SubscribeCommand<CreateNewTenantSubscription>();
        }


        private void OnShutdown()
        {
            this.busSubscriber.Dispose();
        }
    }
}