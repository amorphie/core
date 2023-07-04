using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using amorphie.core.HealthCheck;
using Dapr.Client;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace amorphie.template.HealthCheck
{
    public static class HealthCheckExtension
    {
        public static void AddBBTHealthCheck(this IHealthChecksBuilder builder)
        {
            var serviceProvider = builder.Services.BuildServiceProvider();
            var daprClient = serviceProvider.GetRequiredService<DaprClient>();

            var daprMetadata = GetDaprMetadata(daprClient);
            if (daprMetadata != null && daprMetadata.Components.Any())
                RegisterHealthChecks(builder, daprMetadata.Components, daprClient);
        }

        private static DaprMetadata GetDaprMetadata(DaprClient daprClient)
        {
            return daprClient.GetMetadataAsync(default).GetAwaiter().GetResult();
        }

        private static void RegisterHealthChecks(
            IHealthChecksBuilder healthChecksBuilder,
            IReadOnlyList<DaprComponentsMetadata> components,
            DaprClient daprClient
        )
        {
            foreach (DaprComponentsMetadata daprComponentsMetadata in components)
            {
                if (daprComponentsMetadata.Type.StartsWith("secretstores."))
                {
                    healthChecksBuilder.AddCheck(
                        daprComponentsMetadata.Name,
                        new DaprSecretStoreHealthCheck(daprClient, daprComponentsMetadata.Name),
                        tags: new[] { "SecretStore" }
                    );
                }
                else if (daprComponentsMetadata.Type.StartsWith("pubsub."))
                {
                    healthChecksBuilder.AddCheck(
                        daprComponentsMetadata.Name,
                        new DaprPubSubHealthCheck(daprClient, daprComponentsMetadata.Name),
                        tags: new[] { "PubSub" }
                    );
                }
                else if (daprComponentsMetadata.Type.StartsWith("state."))
                {
                    healthChecksBuilder.AddCheck(
                        daprComponentsMetadata.Name,
                        new DaprStateStoreHealthCheck(daprClient, daprComponentsMetadata.Name),
                        tags: new[] { "StateStore" }
                    );
                }
            }
        }
    }
}
