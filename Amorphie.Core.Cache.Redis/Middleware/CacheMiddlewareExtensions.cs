using Microsoft.AspNetCore.Builder;

namespace Amorphie.Core.Cache.Redis;
public static class CacheMiddlewareExtensions
{
    public static IApplicationBuilder UseDistributedCaching(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CacheMiddleware>();
    }
}
