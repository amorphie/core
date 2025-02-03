using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Amorphie.Core.Cache.Redis;
public class CacheMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IDistributedCache _cache;
    private readonly ILogger<CacheMiddleware> _logger;
    private readonly RedisSettings _redisSettings;

    public CacheMiddleware(
        RequestDelegate next,
        IDistributedCache cache,
        ILogger<CacheMiddleware> logger,
        RedisSettings redisSettings)
    {
        _next = next;
        _cache = cache;
        _logger = logger;
        _redisSettings = redisSettings;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        List<string> headers = new List<string>();
        DistributedCacheAttribute? attribute = null;
        string? overridePathWith = null;
        if (context != null)
        {
            var endPoint = context.GetEndpoint();
            if (endPoint != null)
            {
                attribute = endPoint.Metadata.GetMetadata<DistributedCacheAttribute>();
            }
        }
        var endpointFromConfig = GetEndpointSettings(context.Request.Path);
        if ((attribute == null && endpointFromConfig == null) || !IsGetRequest(context.Request))
        {
            await _next(context);
            return;
        }
        int ttl = 0;
        if (attribute != null)
        {
            ttl = attribute.TimeToLiveMinutes;
            headers = attribute.HeadersToDiffer;
            overridePathWith = attribute.OverridePathWith;

        }
        else if (endpointFromConfig != null)
        {
            ttl = endpointFromConfig.TimeToLiveMinutes;
        }

        var originalBodyStream = context.Response.Body;
        var cacheKey = GenerateCacheKey(context.Request, headers, overridePathWith);

        try
        {
            var cachedResponse = await _cache.GetAsync(cacheKey);
            if (cachedResponse != null)
            {
                context.Response.Headers.Append("FromCache", "true");
                context.Response.ContentType = "application/json";
                await context.Response.Body.WriteAsync(cachedResponse);
                return;
            }

            using var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            await _next(context);

            memoryStream.Position = 0;
            var responseBody = await new StreamReader(memoryStream).ReadToEndAsync();

            if (context.Response.StatusCode == 200)
            {
                _logger.LogDebug("Caching response for key: {CacheKey}", cacheKey);
                await _cache.SetAsync(cacheKey,
                    Encoding.UTF8.GetBytes(responseBody),
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(ttl)
                    });
            }

            memoryStream.Position = 0;
            await memoryStream.CopyToAsync(originalBodyStream);
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }

    private EndpointCacheSettings? GetEndpointSettings(PathString path)
    {
        return _redisSettings.Endpoints.FirstOrDefault(e => e.IsMatch(path.Value ?? string.Empty));
    }

    private static string GenerateCacheKey(HttpRequest request, List<string> headersToDiffer, string? overridePathWith = null)
    {
        var keyBuilder = new StringBuilder();
        if (overridePathWith == null)
            keyBuilder.Append($"{request.Path}");
        else
            keyBuilder.Append(overridePathWith);

        foreach (var (key, value) in request.Query.OrderBy(x => x.Key))
        {
            keyBuilder.Append($"|{key}-{value}");
        }

        foreach (var header in headersToDiffer)
        {
            if (request.Headers.TryGetValue(header, out var headerValue))
            {
                keyBuilder.Append($"|{header}-{headerValue}");
            }
        }


        return keyBuilder.ToString();
    }

    private static bool IsGetRequest(HttpRequest request)
    {
        return request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase);
    }
}
