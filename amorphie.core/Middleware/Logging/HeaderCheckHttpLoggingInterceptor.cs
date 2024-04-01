using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.Options;

namespace amorphie.core.Middleware.Logging;

internal sealed class HeaderCheckHttpLoggingInterceptor : IHttpLoggingInterceptor
{
    private readonly IOptionsMonitor<HttpLoggingOptions> _options;

    public HeaderCheckHttpLoggingInterceptor(IOptionsMonitor<HttpLoggingOptions> options)
    {
        _options = options;
    }

    public ValueTask OnRequestAsync(HttpLoggingInterceptorContext logContext)
    {
        if (logContext.TryDisable(HttpLoggingFields.RequestHeaders))
        {
            LogOnlyIntendedHeaders(logContext);
        }
        return default;
    }

    public ValueTask OnResponseAsync(HttpLoggingInterceptorContext logContext)
    {
        if (logContext.TryDisable(HttpLoggingFields.ResponseHeaders))
        {
            LogOnlyIntendedHeaders(logContext);
        }
        return default;
    }


    /// <summary>
    /// in order to logging control, it is sufficient to use only request headers option for both request and response headers
    /// </summary>
    /// <param name="logContext"></param>
    private void LogOnlyIntendedHeaders(HttpLoggingInterceptorContext logContext)
    {

        var allowedHeaders = _options.CurrentValue.RequestHeaders;
        foreach (var (key, value) in logContext.HttpContext.Request.Headers)
        {
            if (!allowedHeaders.Contains(key))
            {
                continue;
            }
            logContext.AddParameter(key, value.ToString());
        }
    }
}