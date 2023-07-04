
using Dapr.Client;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace amorphie.core.HealthCheck
{
    public class DaprStateStoreHealthCheck : ICustomHealthCheck
    {

        private readonly DaprClient _daprClient;
        public virtual string Name => typeof(DaprStateStoreHealthCheck).Name;
        protected string StateStoreName;

        public DaprStateStoreHealthCheck(DaprClient daprClient, string stateStoreName)
        {
            _daprClient = daprClient;
            StateStoreName = stateStoreName;
        }



        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                await _daprClient.SaveStateAsync(storeName: StateStoreName, key: "healthCheck", value: new { healthCheck = true }, cancellationToken: cancellationToken);
                await _daprClient.GetStateAsync<object>(storeName: StateStoreName, key: "healthCheck", cancellationToken: cancellationToken);
                await _daprClient.DeleteStateAsync(storeName: StateStoreName, key: "healthCheck", cancellationToken: cancellationToken);
                return HealthCheckResult.Healthy($"Dapr state store: {StateStoreName} is healthy.");
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, $"Dapr state store: {StateStoreName} is unhealthy. Error: {ex.Message}");
            }
        }
    }
}
