using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace amorphie.core.Helper;
public static class AmorphieJsonSerializer
{
    private static TextEncoderSettings GetEncoderSettings()
    {
        var encoderSettings = new TextEncoderSettings();
        encoderSettings.AllowCharacters('\u0027', '\u003E');
        encoderSettings.AllowRange(UnicodeRanges.All);
        return encoderSettings;
    }
    private static JsonSerializerOptions opt = new()
    {
        PropertyNameCaseInsensitive = false,
        //WriteIndented = true,
        //Encoder = JavaScriptEncoder.Create(GetEncoderSettings())
        //Encoder = JavaScriptEncoder.Create(new TextEncoderSettings(UnicodeRanges.All))
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static string Serialize<TValue>(TValue value)
    {
        return JsonSerializer.Serialize(value, opt);
    }
    public static TValue? Deserialize<TValue>(string json)
    {
        return JsonSerializer.Deserialize<TValue>(json, opt);
    }
    public static bool TryDeserialize<TValue>(string json, out TValue? result)
    {

        try
        {
            result = JsonSerializer.Deserialize<TValue>(json, opt);

            if (object.Equals(result, default(TValue)))
            {
                return false;
            }
            return true;
        }
        catch
        {
            // same result with parent scope
        }
        result = default;
        return false;
    }
}

