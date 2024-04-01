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
                AddHeaders(httpContext);
                foreach (var query in httpContext.Request.Query)
                {
                    AddPropertyIfAbsent($"query.{query.Key}", query.Value);
                }
                var instanceIdInRoute = httpContext.Request.RouteValues.FirstOrDefault(route => route.Value != null && InstanceId.Equals(route.Key, StringComparison.OrdinalIgnoreCase));
                if (instanceIdInRoute.Value != null)
                {
                    AddPropertyIfAbsent($"correlation.{InstanceId}", instanceIdInRoute.Value!);
                }
                if (httpContext.Items.TryGetValue(InstanceId, out var instanceId) && instanceId != null)
                    AddPropertyIfAbsent($"correlation.{InstanceId}", instanceId);
                if (httpContext.Request.Path.HasValue)
                {
                    AddPropertyIfAbsent($"RequestPath", httpContext.Request.Path.Value);
                }
            }
            catch (Exception ex)
            {
                Log.Fatal($"TraceIdentifier: {httpContext.TraceIdentifier}. Log enrichment with httpcontext props failed. Exception: {ex}");
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
    /// <summary>
    /// Gather up all headers into single field
    /// </summary>
    /// <param name="httpContext"></param>
    private void AddHeaders(HttpContext httpContext)
    {
        var builder = new StringBuilder();
        foreach (var header in httpContext.Request.Headers.Where(header => _options.CurrentValue.RequestHeaders.Contains(header.Key)))
        {
            builder.AppendLine(header.Key + ":" + header.Value);
        }
        if (builder.Length > 0)
        {
            AddPropertyIfAbsent($"headers", builder.ToString());
        }
    }
}


