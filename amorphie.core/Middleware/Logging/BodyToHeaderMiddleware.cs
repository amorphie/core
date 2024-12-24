using System.Text.Json.Nodes;
using System.Text;

namespace amorphie.core.Middleware.Logging;
public class BodyToHeaderMiddleware
{
    private readonly RequestDelegate _next;
    public BodyToHeaderMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Method == "POST")
        {
            var bodyHeaders = await GetHeadersFromBody(context.Request);
            if (bodyHeaders != null)
            {
                AppendRequestHeaders(context.Request, bodyHeaders);
            }
        }
        await _next(context);
    }

    private void AppendRequestHeaders(HttpRequest request, JsonObject bodyHeaders)
    {
        var transaction = Elastic.Apm.Agent.Tracer.CurrentTransaction;
        foreach (var pair in bodyHeaders)
        {
            if (pair.Value != null && LoggingConstants.OptionalHeaders.TryGetValue(pair.Key, out string? headerName) && !string.IsNullOrWhiteSpace(headerName))
            {
                request.Headers.Append(headerName, pair.Value.ToString());
                transaction?.SetLabel($"http_{headerName}", pair.Value.ToString());
            }
        }
    }
    private static async ValueTask<JsonObject?> GetHeadersFromBody(HttpRequest request, Encoding? encoding = null)
    {
        request.EnableBuffering();
        using var reader = new StreamReader(request.Body, encoding ?? Encoding.UTF8, leaveOpen: true);
        string body = await reader.ReadToEndAsync();
        request.Body.Position = 0;
        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }
        try
        {
            var requestAsJson = JsonNode.Parse(body);
            if (requestAsJson == null)
            {
                return null;
            }
            var headers = requestAsJson["Headers"];
            if (headers != null)
            {
                return headers as JsonObject;
            }
        }
        catch
        {
            // if deserialization fails, do nothing and return null
        }
        return null;
    }
}

