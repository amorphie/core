using Refit;

namespace amorphie.core.Refit
{
    public interface IBBTApi<T, in TKey> where T : class
    {
        [Get("/{key}")]
        Task<T> ReadOne(TKey key);

        [Get("?page={page}&pageSize={pageSize}")]
        Task<List<T>> ReadAll([Query] int page, [Query] int pageSize);

        [Post("")]
        Task Upsert([Body] T payload);

        [Delete("/{key}")]
        Task Delete(TKey key);
    }
}
