using amorphie.core.Base.Polly;
using Polly;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;
using Polly.Timeout;
using Refit;
using System.Web;

namespace amorphie.core.Extension
{
    public static class PollyRefitExtensions
    {
        private static IAsyncPolicy<HttpResponseMessage> TimeoutPolicy(PollySettings settings)
    => Policy
        .TimeoutAsync<HttpResponseMessage>( //Catches TaskCanceledException and throws instead TimeoutRejectedException
            timeout: settings.RequestTimeout);


        private static IAsyncPolicy<HttpResponseMessage> CircuitBreakerPolicy(PollySettings settings)
            => HttpPolicyExtensions
                .HandleTransientHttpError() //Catches HttpRequestException or checks the status code: 5xx or 408
                .Or<TimeoutRejectedException>() //Catches TimeoutRejectedException, which can be thrown by an inner TimeoutPolicy
                .CircuitBreakerAsync( //Monitors consecutive failures
                    handledEventsAllowedBeforeBreaking: settings.RequestCircuitBreakerFailCount, //After this amount of consecutive failures it will break
                    durationOfBreak: settings.RequestCircuitBreakerDelay); //After this amount of delay it will give it a try

        private static IAsyncPolicy<HttpResponseMessage> RetryPolicy(PollySettings settings)
            => HttpPolicyExtensions
                .HandleTransientHttpError() //Catches HttpRequestException or checks the status code: 5xx or 408
                .Or<BrokenCircuitException>() //Catches BrokenCircuitException, so whenever the broker is open then it refuses new requests
                .Or<TimeoutRejectedException>() //Catches TimeoutRejectedException, which can be thrown by an inner TimeoutPolicy
                .WaitAndRetryAsync( //Monitors the above anomalies
                    retryCount: settings.RequestRetryCount, //After (this amount + 1) attempts it gives up
                    sleepDurationProvider: _ => settings.RequestRetrySleepDuration); //After a failed attempt it delays the next try with this amount of time


        public static IServiceCollection AddBBTRestClient<TInterface, TImplementation>
            (this IServiceCollection services, PollySettings settings)
            where TInterface : class
            where TImplementation : class, TInterface
        {
            var (serviceUri, combinedPolicy) = CreateRestClientParams<TInterface>(settings);

            services.AddHttpClient<TInterface, TImplementation>(
                    client => { client.BaseAddress = serviceUri; })
                .AddPolicyHandler(combinedPolicy); //Retry > Circuit Breaker > Timeout (outer > inner)


            return services;
        }
        public static IHttpClientBuilder AddBBTRestClient<TInterface>
     (this IServiceCollection services, PollySettings settings)
     where TInterface : class
        {
            var (serviceUri, combinedPolicy) = CreateRestClientParams<TInterface>(settings);

            return services
                .AddRefitClient<TInterface>()
                .ConfigureHttpClient(c => c.BaseAddress = serviceUri)
                .AddPolicyHandler(combinedPolicy); //Retry > Circuit Breaker > Timeout (outer > inner)
        }

        private static (Uri, IAsyncPolicy<HttpResponseMessage>) CreateRestClientParams<TInterface>(PollySettings settings)
        {
            Uri serviceUri = Uri.TryCreate(settings.BaseAddress, UriKind.Absolute, out serviceUri)
                ? serviceUri
                : throw new UriFormatException(
                    $"Invalid url was set for the '{typeof(TInterface).Name}' resilient http client. " +
                    $"Its value was '{HttpUtility.UrlEncode(settings.BaseAddress)}'");

            var combinedPolicy = Policy.WrapAsync(RetryPolicy(settings), CircuitBreakerPolicy(settings), TimeoutPolicy(settings));

            return (serviceUri, combinedPolicy);
        }
    }
}
