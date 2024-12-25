using Amorphie.Core.Cache.Redis;
using Elastic.CommonSchema;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace amorphie.core.test.ui.Modules;
public static class CacheModule
{
    [DistributedCache(5, ["version"])]
    public static async Task<int> GetDefinitionBulkAsync([FromHeader(Name = "version")] string? applicationVersion, [FromQuery(Name ="qs")] string longqs)
    {
        return 100;
    }
}

