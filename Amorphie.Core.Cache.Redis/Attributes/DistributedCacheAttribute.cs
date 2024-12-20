namespace Amorphie.Core.Cache.Redis;

public class DistributedCacheAttribute : Attribute
{
    public int TimeToLiveMinutes { get; set; }
    public List<string> HeadersToDiffer { get; set; }
    public DistributedCacheAttribute(int timeToLiveMinutes)
    {
        HeadersToDiffer = new List<string>();
        TimeToLiveMinutes = timeToLiveMinutes;
    }
    public DistributedCacheAttribute(int timeToLiveMinutes, string[] headersToDiffer)
    {
        TimeToLiveMinutes = timeToLiveMinutes;
        HeadersToDiffer = headersToDiffer.ToList();
    }
}
