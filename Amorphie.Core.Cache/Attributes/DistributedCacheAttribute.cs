namespace Amorphie.Core.Cache.Redis;

public class DistributedCacheAttribute : Attribute
{
    public int TimeToLiveMinutes { get; set; }
    public List<string> HeadersToDiffer { get; set; } = new List<string>();
    public string? OverridePathWith { get; set; }
    public string? CacheFormat { get; set; }

    public DistributedCacheAttribute(
        int timeToLiveMinutes, 
        string[]? headersToDiffer = null, 
        string? overridePathWith = null,
        string? cacheFormat = null)
    {
        TimeToLiveMinutes = timeToLiveMinutes;
        OverridePathWith = overridePathWith;
        CacheFormat = cacheFormat;
        HeadersToDiffer ??= headersToDiffer.ToList();
    }
}
