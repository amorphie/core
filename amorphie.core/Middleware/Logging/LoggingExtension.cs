using Microsoft.Extensions.DependencyInjection.Extensions;
using Serilog.Core;
using Serilog;
using Elastic.Apm.SerilogEnricher;

namespace amorphie.core.Middleware.Logging;
public static class LoggingExtension
{
    public static void UseLoggingHandlerMiddlewares(this WebApplication app)
    {
        var loggingOptions = new LoggingOptions();
        var loggingSection = app.Configuration.GetSection(LoggingOptions.Logging);
        loggingSection.Bind(loggingOptions);
        var sanitizeFieldNames = loggingSection.GetValue<string>(nameof(LoggingOptions.SanitizeFieldNames));
        loggingOptions.SanitizeFieldNames = sanitizeFieldNames?.Split(',');
        var sanitizeHeaderNames = loggingSection.GetValue<string>(nameof(LoggingOptions.SanitizeHeaderNames));
        loggingOptions.SanitizeHeaderNames = sanitizeHeaderNames?.Split(',');
        var ignorePaths = loggingSection.GetValue<string>(nameof(LoggingOptions.IgnorePaths));
        loggingOptions.IgnorePaths = ignorePaths?.Split(',');
        app.UseMiddleware<LoggingMiddleware>(loggingOptions);

    }

    public static void AddSeriLog<TEnricher>(this WebApplicationBuilder builder) where TEnricher : class, ILogEventEnricher
    {

        builder.Services.AddHttpContextAccessor();
        builder.Services.TryAddSingleton<ILogEventEnricher, TEnricher>();

        builder.Logging.ClearProviders();

        builder.Host.UseSerilog((_, serviceProvider, loggerConfiguration) =>
        {
            var enricher = serviceProvider.GetRequiredService<ILogEventEnricher>();
            loggerConfiguration
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.With(enricher)
                .Enrich.WithElasticApmCorrelationInfo()
                ;
        });
    }
}

