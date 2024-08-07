using System.Text.Encodings.Web;
using System.Text.Json;

namespace amorphie.core.Middleware.Logging;
internal static class LoggingJsonSerializer
{
    private static JsonSerializerOptions opt = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        //WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    internal static string Serialize<TValue>(TValue value)
    {
        return JsonSerializer.Serialize(value, opt);
    }
    internal static TValue? Deserialize<TValue>(string json)
    {
        return JsonSerializer.Deserialize<TValue>(json, opt);
    }
}


