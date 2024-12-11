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
        loggingOptions.IgnorePaths = ignorePaths?.Split(',', StringSplitOptions.TrimEntries);
        app.UseMiddleware<LoggingMiddleware>(loggingOptions);

    }
    public static void AddSeriLog(this WebApplicationBuilder builder)
    {
        AddSeriLogPrivate(builder);
    }
    public static void AddSeriLog<TEnricher>(this WebApplicationBuilder builder) where TEnricher : class, ILogEventEnricher
    {
        //amorphie log enricher is the default enricher and will be added by default
        //if the enricher is not amorphie log enricher then add the custom enricher too
        if (typeof(TEnricher) == typeof(AmorphieLogEnricher))
        {
            AddSeriLogPrivate(builder);
        }
        else
        {
            builder.Services.TryAddSingleton<ILogEventEnricher, TEnricher>();
            AddSeriLogPrivate(builder, hasCustomEventEnricher: true);
        }
    }
    private static void AddSeriLogPrivate(WebApplicationBuilder builder, bool? hasCustomEventEnricher = null)
    {
        builder.Services.AddHttpContextAccessor();
        builder.Services.TryAddSingleton<AmorphieLogEnricher>();
        //https://github.com/dotnet/aspnetcore/issues/48355
        builder.Services.Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = true);
        builder.Logging.ClearProviders();

        builder.Host.UseSerilog((_, serviceProvider, loggerConfiguration) =>
        {
            var amorphieEnricher = serviceProvider.GetRequiredService<AmorphieLogEnricher>();
            loggerConfiguration
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.With(amorphieEnricher)
                .Enrich.WithElasticApmCorrelationInfo()
                ;
            if (hasCustomEventEnricher == true)
            {
                var enricher = serviceProvider.GetRequiredService<ILogEventEnricher>();
                loggerConfiguration.Enrich.With(enricher);
            }
        });
    }

}

