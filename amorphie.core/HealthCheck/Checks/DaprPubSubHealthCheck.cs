
using Dapr.Client;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace amorphie.core.HealthCheck
{
    public class DaprPubSubHealthCheck : ICustomHealthCheck
    {
        private readonly DaprClient _daprClient;
        public virtual string Name => typeof(DaprPubSubHealthCheck).Name;
        protected string PubSubName;

        public DaprPubSubHealthCheck(DaprClient daprClient, string pubSubName)
        {
            _daprClient = daprClient;
            PubSubName = pubSubName;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                var topicName = $"Dapr-{"healthCheckTopic"}-{PubSubName}";
                var timeToLeave = 60;

                await _daprClient.PublishEventAsync(PubSubName, topicName, default);

                return HealthCheckResult.Healthy(
                    $"Dapr pubsub: {PubSubName} for topic '{topicName}' is healthy."
                );
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(
                    context.Registration.FailureStatus,
                    $"Dapr pubsub: {PubSubName} is unhealthy. Error: {ex.Message}"
                );
            }
        }
    }
}
