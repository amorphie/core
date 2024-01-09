using System;
using System.Reflection;
using System.Reflection.PortableExecutable;
using amorphie.core.Serilog.Enricher;
using Dapr.Client;
using Serilog;
using Serilog.Exceptions;
using Serilog.Filters;
using Serilog.Formatting.Elasticsearch;
using Serilog.Formatting.Json;
using Serilog.Sinks.Elasticsearch;

namespace amorphie.core.Extension
{

    public static class SerilogExtension
    {
        public static async Task AddVaultSecrets(this WebApplicationBuilder builder, string? indexFormat)
        {
            builder.Services.AddHttpContextAccessor();

            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var applicationName = Assembly.GetExecutingAssembly().GetName().Name;

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .Enrich.With<SerilogAmorphieEnricher>()
                .Enrich.WithExceptionDetails()
                .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore.StaticFiles"))
                .WriteTo.Async(writeTo => writeTo.Console(new JsonFormatter()))
                .WriteTo.Async(writeTo => writeTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(builder.Configuration.GetValue<string>("ElasticConfiguration:uri")))
                {
                    TypeName = null,
                    AutoRegisterTemplate = true,
                    IndexFormat = indexFormat,
                    BatchAction = ElasticOpType.Create,
                    CustomFormatter = new ElasticsearchJsonFormatter(),
                    OverwriteTemplate = true,
                    DetectElasticsearchVersion = true,
                    AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
                    NumberOfReplicas = 1,
                    NumberOfShards = 2,
                    FailureCallback = e => Console.WriteLine("Unable to submit event " + e.MessageTemplate),
                    EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog |
                                                               EmitEventFailureHandling.WriteToFailureSink |
                                                               EmitEventFailureHandling.RaiseCallback |
                                                               EmitEventFailureHandling.ThrowException
                }))
               .CreateLogger();

            builder.Logging.ClearProviders();
            builder.Host.UseSerilog(Log.Logger, true);
        }
    }
}

