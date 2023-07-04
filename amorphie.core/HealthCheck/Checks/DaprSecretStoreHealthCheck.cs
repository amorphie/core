
using Dapr.Client;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.VisualBasic;

namespace amorphie.core.HealthCheck
{
    public class DaprSecretStoreHealthCheck : ICustomHealthCheck
    {
        private readonly DaprClient _daprClient;
        public string Name => typeof(DaprSecretStoreHealthCheck).Name;
        protected string SecretStoreName;

        public DaprSecretStoreHealthCheck(DaprClient daprClient, string secretStoreName)
        {
            _daprClient = daprClient;
            SecretStoreName = secretStoreName;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                var secret = await _daprClient
                    .GetBulkSecretAsync(
                        storeName: SecretStoreName,
                        cancellationToken: cancellationToken
                    )
                    .ConfigureAwait(false);
                if (secret is null)
                    return new HealthCheckResult(
                        context.Registration.FailureStatus,
                        "Dapr secret store is unhealthy"
                    );
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(
                    context.Registration.FailureStatus,
                    $"Dapr secret store is unhealthy. Exception: {ex.Message}"
                );
            }

            return HealthCheckResult.Healthy($"Dapr secret store: {SecretStoreName} is healthy.");
        }
    }
}
