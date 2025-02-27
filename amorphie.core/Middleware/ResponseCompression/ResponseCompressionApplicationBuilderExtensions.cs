using Microsoft.Extensions.Options;

namespace amorphie.core.Middleware.ResponseCompression;

public static class ResponseCompressionApplicationBuilderExtensions
{
    public static IApplicationBuilder UseCompression(this IApplicationBuilder app)
    {
        var options = app.ApplicationServices.GetService<IOptions<ResponseCompressionOptions>>();
        if (options?.Value is { Enable: true })
        {
            return app.UseResponseCompression();
        }

        return app;
    }
}