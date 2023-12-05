using System;
using Dapr.Client;

namespace amorphie.core.Extension;

	public static class DaprClientExtensions
{
    public static async Task<TModel> CacheReadWrite<TModel>(this DaprClient daprClient, Func<Task<TModel>> readAction, string cacheStoreName, string cacheKey, int TTLSeconds, string lockStoreName)
    {
        TModel result = await daprClient.GetStateAsync<TModel>(cacheStoreName, cacheKey);

        if (result == null)
        {
            string lockKey = $"{cacheKey}_lock";
            await using (var fileLock = await daprClient.Lock(lockStoreName, lockKey, Guid.NewGuid().ToString(), 60))
            {
                if (fileLock.Success)
                {
                    result = await readAction();

                    Dictionary<string, string> metadata = null;
                    if (TTLSeconds > 0)
                    {
                        metadata = new Dictionary<string, string>() { { "ttlInSeconds", TTLSeconds.ToString() } };
                    }
                    await daprClient.SaveStateAsync<TModel>("amorphie-state", cacheKey, result, metadata: metadata);

                }
                else
                {
                    Console.WriteLine($"Failed to lock {lockKey}.");
                }
            }

        }
        return result;
    }
    public static async Task<TModel> CacheRead<TModel>(this DaprClient daprClient, string cacheStoreName, string cacheKey)
    {
        return await daprClient.GetStateAsync<TModel>(cacheStoreName, cacheKey);
    }
    public static async Task CacheDelete(this DaprClient daprClient, string cacheStoreName, string cacheKey, string lockStoreName)
    {
        string lockKey = $"{cacheKey}_lock";
        await using (var fileLock = await daprClient.Lock(lockStoreName, lockKey, Guid.NewGuid().ToString(), 60))
        {
            if (fileLock.Success)
            {
                await daprClient.DeleteStateAsync(cacheStoreName, cacheKey);
            }
            else
            {
                Console.WriteLine($"Failed to lock {lockKey}.");
            }
        }
    }

}


