using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Amorphie.Core.Cache.Redis;

public class CacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly IConnectionMultiplexer _redisConnection;
    private readonly ILogger<CacheService> _logger;
    private readonly string _instanceName;

    public CacheService(
        IDistributedCache cache,
        IConnectionMultiplexer redisConnection,
        IConfiguration configuration,
        ILogger<CacheService> logger)
    {
        _cache = cache;
        _redisConnection = redisConnection;
        _logger = logger;
        _instanceName = configuration.GetSection("Redis:InstanceName").Value ?? "DistributedCache_";
    }

    public async Task<bool> InvalidateByKeyAsync(string cacheKey)
    {
        try
        {
            string fullKey = $"{_instanceName}{cacheKey}";
            await _cache.RemoveAsync(cacheKey);
            _logger.LogInformation("Cache invalidated for key: {CacheKey}", cacheKey);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating cache for key: {CacheKey}", cacheKey);
            return false;
        }
    }

    public async Task<int> InvalidateByPatternAsync(string pattern)
    {
        try
        {
            var keys = await GetAllKeysAsync(pattern);
            var count = 0;

            foreach (var key in keys)
            {
                // Remove the instance name prefix to get the original cache key
                var cacheKey = key.Substring(_instanceName.Length);
                await _cache.RemoveAsync(cacheKey);
                count++;
            }

            _logger.LogInformation("Invalidated {Count} cache entries matching pattern: {Pattern}", count, pattern);
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating cache for pattern: {Pattern}", pattern);
            return 0;
        }
    }

    public async Task<IEnumerable<string>> GetAllKeysAsync(string pattern = "*")
    {
        var keys = new List<string>();
        pattern = $"{_instanceName}{pattern}";
        foreach (var endpoint in _redisConnection.GetEndPoints())
        {
            var server = _redisConnection.GetServer(endpoint);
            await foreach (var key in server.KeysAsync(pattern: pattern, flags: CommandFlags.PreferMaster))
            {
                keys.Add(key.ToString());
            }
        }
        return keys;
    }
}
