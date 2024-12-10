using Serilog.Core;
using Serilog.Events;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.Options;
using Serilog;
using System.Text;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace amorphie.core.Middleware.Logging;

public class AmorphieLogEnricher : ILogEventEnricher
{
    private const string InstanceId = "InstanceId";
    private readonly IHttpContextAccessor _httpContextAccessor;

    private readonly string[] wild = { "Authorization", "Password" };

    private readonly string[] _optionalHeaders = {"user_reference",
        "X-Subject",
        "X-Device-Id",
         "x-device-id",
        "X-Token-Id",
        "x-token-id",
        "X-Customer",
        "X-Workflow-Name",
        "X-Instance-Id",
        "x-instance-id",
        "X-Request-Id",
        "x-request-id",
        "X-Installation-Id",
        "x-installation-id",
        "X-Application",
        "Clientipaddress",
        "Clientid",
        "Jti",
        "X-Session-Id"};

    private readonly IOptionsMonitor<HttpLoggingOptions> _options;

    public AmorphieLogEnricher(IHttpContextAccessor httpContextAccessor, IOptionsMonitor<HttpLoggingOptions> options)
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
                if (instanceId != null)
                {
                    AddPropertyIfAbsent($"correlation.{InstanceId}", instanceId);
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
                        AddPropertyIfAbsent(headerKey, headerValue.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Fatal("TraceIdentifier: {TraceIdentifier}. Log enrichment with httpcontext props failed. Exception: {ex}", httpContext.TraceIdentifier, ex);
            }
        }

        AddPropertyIfAbsent("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "None");
        AddPropertyIfAbsent("ApplicationName", Environment.GetEnvironmentVariable("ApplicationName") ?? "None");
    }
    void AddPropertyIfAbsent(string key, object value)
    {
        if (wild.Contains(key))
            value = "******";

        _logEvent.AddPropertyIfAbsent(_propertyFactory.CreateProperty(key, value, true));
    }

}