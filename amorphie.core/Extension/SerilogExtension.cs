using System;
using System.Reflection;
using System.Reflection.PortableExecutable;
using amorphie.core.Serilog.Enricher;
using Dapr.Client;
using Microsoft.Extensions.Configuration;
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
            public static void AddSeriLog(
            this WebApplicationBuilder builder)
        {

            builder.Services.AddHttpContextAccessor();

            Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .CreateLogger();

            builder.Logging.ClearProviders();

            builder.Host.UseSerilog(Log.Logger, true);
        }
    }
}

