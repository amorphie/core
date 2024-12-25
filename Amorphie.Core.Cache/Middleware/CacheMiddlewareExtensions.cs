using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Amorphie.Core.Cache.Redis;
public static class CacheMiddlewareExtensions
{
    public static void AddDistributedCaching(this WebApplicationBuilder builder)
    {
        var redisSettings = builder.Configuration.GetSection("Redis").Get<RedisSettings>() ?? throw new InvalidOperationException("Redis settings are not configured");
        builder.Services.AddSingleton(redisSettings);
        // Configure Redis Connection
        builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(redisSettings.Configuration));

        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisSettings.Configuration;
            options.InstanceName = redisSettings.InstanceName;
        });
        builder.Services.AddScoped<ICacheService, CacheService>();

    }
    public static IApplicationBuilder UseDistributedCaching(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CacheMiddleware>();
    }
}
