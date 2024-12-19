namespace Amorphie.Core.Cache.Redis;
public interface ICacheService
{
    Task<bool> InvalidateByKeyAsync(string cacheKey);
    Task<int> InvalidateByPatternAsync(string pattern);
    Task<IEnumerable<string>> GetAllKeysAsync(string pattern = "*");
}