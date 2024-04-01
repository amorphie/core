using Microsoft.Extensions.DependencyInjection.Extensions;
using Serilog.Core;
using Serilog;
using Microsoft.AspNetCore.HttpLogging;
using Elastic.Apm.SerilogEnricher;
using amorphie.core.Extension;

namespace amorphie.core.Middleware.Logging;
public static class LoggingExtension
{
    public static void UseLoggingHandlerMiddlewares(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionHandlerMiddleware>();
        //Request and Response Logging
        app.UseHttpLogging();
    }

    public static void AddSeriLogWithHttpLogging<TEnricher>(this WebApplicationBuilder builder, List<string>? headersToBeLogged = null) where TEnricher : class, ILogEventEnricher
    {
        var defaultHeadersToBeLogged = new List<string>
        {
            "Content-Type",
            "Host",
            "X-Zeebe-Job-Key",
            "xdeviceid",
            "X-Device-Id",
            "xtokenid",
            "X-Token-Id",
            "Transfer-Encoding",
            "X-Forwarded-Host",
            "X-Forwarded-For"
        };
        if (headersToBeLogged != null)
            defaultHeadersToBeLogged = defaultHeadersToBeLogged.Concat(headersToBeLogged).ToList();


        builder.Services.AddHttpLogging(logging =>
        {
            //logs just props and headers in prod mode
            var isProd = builder.Environment.IsProd() || builder.Environment.IsProduction();
            logging.LoggingFields = isProd ? HttpLoggingFields.RequestScheme : HttpLoggingFields.RequestBody | HttpLoggingFields.ResponseBody | HttpLoggingFields.RequestScheme;
            defaultHeadersToBeLogged.ForEach(p => logging.RequestHeaders.Add(p));
            logging.MediaTypeOptions.AddText("application/javascript");
            logging.RequestBodyLogLimit = 4096;
            logging.ResponseBodyLogLimit = 4096;
            logging.CombineLogs = true;
        });
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
        builder.Services.AddHttpLoggingInterceptor<HeaderCheckHttpLoggingInterceptor>();
    }
}

