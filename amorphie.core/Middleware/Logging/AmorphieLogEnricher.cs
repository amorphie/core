using Serilog.Core;
using Serilog.Events;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.Options;
using Serilog;

namespace amorphie.core.Middleware.Logging;

public class AmorphieLogEnricher : ILogEventEnricher
{
    private const string InstanceId = "InstanceId";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AmorphieLogEnricher(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext is not null)
        {
            try
            {               
                if (httpContext.Request.Path.HasValue)
                {
                    logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("RequestPath", $"{httpContext.Request.Path.Value}{httpContext.Request.QueryString}", true));
                }
                // Log specified optional headers
                foreach (var header in LoggingConstants.OptionalHeaders)
                {
                    if (httpContext.Request.Headers.TryGetValue(header.Value, out var headerValue))
                    {
                        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(header.Value, headerValue.ToString(), true));
                    }
                }

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
                    logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("X-Instance-Id", instanceId, true));
                }
            }
            catch (Exception ex)
            {
                Log.Fatal("TraceIdentifier: {TraceIdentifier}. Log enrichment with httpcontext props failed. Exception: {ex}", httpContext.TraceIdentifier, ex);
            }
        }
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "None", true));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ApplicationName", Environment.GetEnvironmentVariable("ApplicationName") ?? "None", true));
    }
}