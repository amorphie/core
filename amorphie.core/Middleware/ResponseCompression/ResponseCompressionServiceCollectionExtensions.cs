using System.IO.Compression;
using Microsoft.AspNetCore.ResponseCompression;

namespace amorphie.core.Middleware.ResponseCompression;

public static class ResponseCompressionServiceCollectionExtensions
{
    public static IServiceCollection AddCompression(this IServiceCollection services,
        IConfiguration configuration)
    {
        var responseCompressionConfig = configuration.GetSection("ResponseCompression");
        services.Configure<ResponseCompressionOptions>(responseCompressionConfig);
        var enableCompression = responseCompressionConfig.GetValue<bool>("Enable");

        if (!enableCompression)
        {
            return services;
        }

        var mimeTypes = responseCompressionConfig.GetSection("MimeTypes").Get<string[]>() ?? [];
        var excludedMimeTypes = responseCompressionConfig.GetSection("ExcludedMimeTypes").Get<string[]>() ?? [];
        var providers = responseCompressionConfig.GetSection("Providers").Get<string[]>() ?? [];

        services.AddResponseCompression(options =>
        {
            options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(mimeTypes);
            options.ExcludedMimeTypes = excludedMimeTypes;
            options.EnableForHttps = responseCompressionConfig.GetValue<bool>("EnableForHttps", true);
            foreach (var provider in providers)
            {
                switch (provider.ToLowerInvariant())
                {
                    case "gzip":
                        options.Providers.Add<GzipCompressionProvider>();
                        break;
                    case "brotli":
                        options.Providers.Add<BrotliCompressionProvider>();
                        break;
                }
            }
        });

        if (providers.Contains("gzip", StringComparer.OrdinalIgnoreCase))
        {
            services.Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Optimal;
            });
        }

        if (providers.Contains("brotli", StringComparer.OrdinalIgnoreCase))
        {
            services.Configure<BrotliCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Optimal;
            });
        }

        return services;
    }
}