using System.Security.Cryptography.X509Certificates;
using Autofac;
using ChargeBee.Api;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Ranger.ApiUtilities;
using Ranger.Common;
using Ranger.InternalHttpClient;
using Ranger.Monitoring.HealthChecks;
using Ranger.RabbitMQ;
using Ranger.Services.Operations.Messages.Subscriptions.Commands;
using Ranger.Services.Operations.Messages.Subscriptions.RejectedEvents;
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
                    options.Filters.Add<OperationCanceledExceptionFilter>();
                })
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                });

            services.AddRangerApiVersioning();
            services.ConfigureAutoWrapperModelStateResponseFactory();

            services.AddSwaggerGen("Subscriptions API", "v1");

            var identityAuthority = configuration["httpClient:identityAuthority"];
            services.AddPollyPolicyRegistry();
            services.AddTenantsHttpClient("http://tenants:8082", identityAuthority, "tenantsApi", "cKprgh9wYKWcsm");
            services.AddProjectsHttpClient("http://projects:8086", identityAuthority, "projectsApi", "usGwT8Qsp4La2");
            services.AddIdentityHttpClient("http://identity:5000", identityAuthority, "IdentityServerApi", "89pCcXHuDYTXY");
            services.AddGeofencesHttpClient("http://geofences:8085", identityAuthority, "geofencesApi", "9pwJgpgpu6PNJi");
            services.AddIntegrationsHttpClient("http://integrations:8087", identityAuthority, "integrationsApi", "6HyhzSoSHvxTG");

            services.AddDbContext<SubscriptionsDbContext>(options =>
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
                .SetApplicationName("Subscriptions")
                .ProtectKeysWithCertificate(new X509Certificate2(configuration["DataProtectionCertPath:Path"]))
                .UnprotectKeysWithAnyCertificate(new X509Certificate2(configuration["DataProtectionCertPath:Path"]))
                .PersistKeysToDbContext<SubscriptionsDbContext>();

            services.AddTransient<SubscriptionsRepository>();

            services.AddLiveHealthCheck();
            services.AddEntityFrameworkHealthCheck<SubscriptionsDbContext>();
            services.AddDockerImageTagHealthCheck();
            services.AddRabbitMQHealthCheck();
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterInstance<ChargeBeeOptions>(configuration.GetOptions<ChargeBeeOptions>("chargeBee"));
            builder.RegisterType<SubscriptionsService>();
            builder.AddRabbitMq();
        }

        public void Configure(IApplicationBuilder app, IHostApplicationLifetime applicationLifetime)
        {
            ApiConfig.Configure(configuration.GetOptions<ChargeBeeOptions>("chargeBee").Site, configuration.GetOptions<ChargeBeeOptions>("chargeBee").ApiKey);
            app.UseSwagger("v1", "Subscriptions API");
            app.UseAutoWrapper();
            app.UseUnhandedExceptionLogger();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks();
                endpoints.MapLiveTagHealthCheck();
                endpoints.MapEfCoreTagHealthCheck();
                endpoints.MapDockerImageTagHealthCheck();
                endpoints.MapRabbitMQHealthCheck();
            });
            this.busSubscriber = app.UseRabbitMQ()
                .SubscribeCommand<CreateNewTenantSubscription>((c, e) =>
                    new NewTenantSubscriptionRejected(e.Message, ""))
                .SubscribeCommand<UpdateTenantSubscriptionOrganization>((c, e) =>
                    new UpdateTenantSubscriptionOrganizationRejected(e.Message, ""))
                .SubscribeCommand<CancelTenantSubscription>((c, e) =>
                    new CancelTenantSubscriptionRejected(e.Message, ""))
                .SubscribeCommand<UpdateSubscription>()
                .SubscribeCommand<ComputeTenantLimitDetails>();
        }
    }
}