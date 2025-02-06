using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapr.Client;
using Microsoft.Extensions.Logging;

namespace Amorphie.Core.Cache.Dapr
{
    public class DaprCacheService : IDaprCacheService
    {
        private readonly ILogger<DaprCacheService> _logger;
        private readonly DaprClient _daprClient;

        public DaprCacheService(DaprClient daprClient, ILogger<DaprCacheService> logger)
        {
            _logger = logger;
            _daprClient = daprClient;
        }

        public async Task<TValue> GetStateAsync<TValue>(
            string storeName, 
            string key, 
            IReadOnlyDictionary<string, string> metadata = null, 
            bool throwException = true, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await _daprClient.GetStateAsync<TValue>(storeName, key, metadata: metadata, cancellationToken:cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "An error occurred in DaprState operation {key} {state} {throwException}", key, "GetState", throwException);
                if (throwException) throw;
            }

            return default;
        }

        public async Task SaveStateAsync<TValue>(
            string storeName, 
            string key, 
            TValue value, 
            IReadOnlyDictionary<string, string> metadata = null, 
            bool throwException = true, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                await _daprClient.SaveStateAsync(storeName, key, value, metadata: metadata, cancellationToken: cancellationToken);
                _logger.LogInformation("Cache Set {storeName} {key} {ttlInSeconds}", storeName, key, metadata?.FirstOrDefault(x => x.Key == "ttlInSeconds").Value);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "An error occurred in DaprState operation {key} {state} {throwException}", key, "SaveState", throwException);
                if (throwException) throw;
            }
        }

        public async Task DeleteStateAsync(
            string storeName, 
            string key, 
            IReadOnlyDictionary<string, string> metadata = null, 
            bool throwException = true, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                await _daprClient.DeleteStateAsync(storeName, key, metadata:metadata, cancellationToken:cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "An error occurred in DaprState operation {key} {state} {throwException}", key, "DeleteState", throwException);
                if (throwException) throw;
            }
        }

       public async Task<TValue> GetOrSetStateAsync<TValue>(
            string storeName, 
            string key, 
            Func<Task<TValue>> getDataFunc, 
            IReadOnlyDictionary<string, string> metadata = null, 
            bool throwException = true, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var cachedValue = await GetStateAsync<TValue>(storeName, key, metadata, throwException, cancellationToken);

                if (cachedValue != null)
                {
                    return cachedValue;
                }

                // Cache is empty, fetch from database
                var value = await getDataFunc();

                // Save to cache only if value is not null
                if (value != null)
                {
                    await SaveStateAsync(storeName, key, value, metadata, throwException, cancellationToken);
                }

                return value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in GetOrSetStateAsync for {key} in {storeName}", key, storeName);
                if (throwException) throw;
            }

            return default;
        }
    }
}
