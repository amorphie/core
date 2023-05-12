namespace amorphie.core.Base.Polly
{
    public class PollySettings
    {
        public string BaseAddress { get; set; }

        public TimeSpan RequestTimeout { get; set; }

        public TimeSpan RequestRetrySleepDuration { get; set; }

        public int RequestRetryCount { get; set; }

        public int RequestCircuitBreakerFailCount { get; set; }

        public TimeSpan RequestCircuitBreakerDelay { get; set; }
    }
}
