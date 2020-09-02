using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ranger.Common;
using Ranger.InternalHttpClient;
using Ranger.Services.Subscriptions.Data;

namespace Ranger.Services.Subscriptions
{
    public class SubscriptionsService
    {
        private readonly IProjectsHttpClient projectsHttpClient;
        private readonly IIdentityHttpClient identityHttpClient;
        private readonly IGeofencesHttpClient geofencesHttpClient;
        private readonly IIntegrationsHttpClient integrationsHttpClient;
        private readonly SubscriptionsRepository subscriptionsRepository;
        private readonly ILogger<SubscriptionsRepository> logger;

        public SubscriptionsService(
            IProjectsHttpClient projectsHttpClient,
            IIdentityHttpClient identityHttpClient,
            IGeofencesHttpClient geofencesHttpClient,
            IIntegrationsHttpClient integrationsHttpClient,
            SubscriptionsRepository subscriptionsRepository,
            ILogger<SubscriptionsRepository> logger)
        {
            this.logger = logger;
            this.projectsHttpClient = projectsHttpClient;
            this.identityHttpClient = identityHttpClient;
            this.geofencesHttpClient = geofencesHttpClient;
            this.integrationsHttpClient = integrationsHttpClient;
            this.subscriptionsRepository = subscriptionsRepository;
        }

        public async Task<PlanLimits> GetUtilizedLimitFields(string tenantId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var geofenceCount = await geofencesHttpClient.GetAllActiveGeofencesCount(tenantId, cancellationToken);
            var integrationCount = await integrationsHttpClient.GetAllActiveIntegrationsCount(tenantId, cancellationToken);
            var projects = await projectsHttpClient.GetAllProjects<IEnumerable<Project>>(tenantId, cancellationToken);
            var userCount = await identityHttpClient.GetAllUsersAsync<IEnumerable<User>>(tenantId, cancellationToken);
            return new PlanLimits
            {
                Geofences = (int)geofenceCount.Result,
                Integrations = integrationCount.Result,
                Projects = projects.Result.Count(),
                Accounts = userCount.Result.Count()
            };
        }
    }
}