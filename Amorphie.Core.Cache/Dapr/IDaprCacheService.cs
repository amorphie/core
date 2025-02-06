using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Amorphie.Core.Cache.Dapr
{
    public interface IDaprCacheService
{
    Task<TValue> GetStateAsync<TValue>(
        string storeName, 
        string key, 
        IReadOnlyDictionary<string, string> metadata = null, 
        bool throwException = true, 
        CancellationToken cancellationToken = default);

    Task SaveStateAsync<TValue>(
        string storeName, 
        string key, 
        TValue value, 
        IReadOnlyDictionary<string, string> metadata = null, 
        bool throwException = true, 
        CancellationToken cancellationToken = default);

    Task DeleteStateAsync(
        string storeName, 
        string key, 
        IReadOnlyDictionary<string, string> metadata = null, 
        bool throwException = true, 
        CancellationToken cancellationToken = default);

    Task<TValue> GetOrSetStateAsync<TValue>(
        string storeName, 
        string key, 
        Func<Task<TValue>> getDataFunc, 
        IReadOnlyDictionary<string, string> metadata = null, 
        bool throwException = true, 
        CancellationToken cancellationToken = default);
}

}