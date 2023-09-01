using Refit;

namespace amorphie.core.Refit
{
    public interface IBBTTemplateEngine<TRequest, TResponse>
    {
        [Post("/Template/Render")]
        Task<TResponse> Send([Body] TRequest payload);
    }
}
