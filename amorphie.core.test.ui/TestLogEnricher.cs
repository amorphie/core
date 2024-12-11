using Serilog.Core;
using Serilog.Events;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.Options;
using Serilog;

namespace amorphie.workflow.core.test.ui;

public class TestLogEnricher : ILogEventEnricher
{
    private const string InstanceId = "InstanceId";
    private readonly IHttpContextAccessor _httpContextAccessor;

    private readonly string[] wild = { "Authorization", "Password" };

    private readonly string[] _optionalHeaders = { "accept" };

    private readonly IOptionsMonitor<HttpLoggingOptions> _options;

    public TestLogEnricher(IHttpContextAccessor httpContextAccessor, IOptionsMonitor<HttpLoggingOptions> options)
    {
        _httpContextAccessor = httpContextAccessor;
        _options = options;
    }

    private LogEvent _logEvent;
    private ILogEventPropertyFactory _propertyFactory;

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        _logEvent = logEvent;
        _propertyFactory = propertyFactory;

        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext is not null)
        {
            try
            {
                object? instanceId = null;
                var instanceIdInRoute = httpContext.Request.RouteValues.FirstOrDefault(route => route.Value != null && InstanceId.Equals(route.Key, StringComparison.OrdinalIgnoreCase));
                if (instanceIdInRoute.Value != null)
                {
                    instanceId = instanceIdInRoute.Value;
                }
                if (instanceId == null)
                {
                    httpContext.Items.TryGetValue(InstanceId, out instanceId);
                }
                if (httpContext.Request.Path.HasValue)
                {
                    _logEvent.AddOrUpdateProperty(_propertyFactory.CreateProperty("RequestPath", $"{httpContext.Request.Path.Value}{httpContext.Request.QueryString}", true));

                }
                // Log specified optional headers
                foreach (var headerKey in _optionalHeaders)
                {
                    if (httpContext.Request.Headers.TryGetValue(headerKey, out var headerValue))
                    {
                        _logEvent.AddPropertyIfAbsent(_propertyFactory.CreateProperty(headerKey, headerValue, true));
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Fatal("TraceIdentifier: {TraceIdentifier}. Log enrichment with httpcontext props failed. Exception: {ex}", httpContext.TraceIdentifier, ex);
            }
        }
    }
}


