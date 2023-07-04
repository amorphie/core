using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace amorphie.core.HealthCheck
{
    public interface ICustomHealthCheck : IHealthCheck
    {
        public string Name { get; }
    }
}
