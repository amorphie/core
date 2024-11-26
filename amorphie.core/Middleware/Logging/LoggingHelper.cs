using System.Text.Json.Nodes;
using System.Text.Json;

namespace amorphie.core.Middleware.Logging;

public static class LoggingHelper
{
    private static string[]? _redactKeys;
    public static string FilterContent(string responseBodyText, string[] redactKeys)
    {
        if (string.IsNullOrEmpty(responseBodyText))
        {
            return string.Empty;
        }
        _redactKeys = redactKeys;
        try
        {
            var responseAsJson = LoggingJsonSerializer.Deserialize<JsonObject>(responseBodyText);
            if (responseAsJson == null || redactKeys.Length == 0)
            {
                return responseBodyText;
            }
            var keys = responseAsJson.Select(p => p.Key).ToList();
            foreach (var key in keys)
            {
                if (redactKeys.Contains(key))
                {
                    responseAsJson[key] = "***";
                    continue;
                }
                if (responseAsJson[key] is not null)
                {
                    var valueKind = responseAsJson[key]!.GetValueKind();
                    if (valueKind == JsonValueKind.Object)
                    {
                        var innerDict = responseAsJson[key] as IDictionary<string, JsonNode>;
                        if (innerDict != null)
                        {
                            var decResult = FilterDictionary(innerDict);
                            responseAsJson[key] = decResult as JsonObject;

                        }
                    }
                    else if (valueKind == JsonValueKind.String || valueKind == JsonValueKind.Number)
                    {
                        var valueToBeRedacted = responseAsJson[key]!.ToString();
                        if (valueToBeRedacted.StartsWith('{'))
                        {
                            try
                            {
                                responseAsJson[key] = FilterContent(responseAsJson[key]!.ToString(), redactKeys);
                            }
                            catch
                            {
                                //Do nothing
                            }
                        }
                        else
                        {
                            responseAsJson[key] = FilterString(key, valueToBeRedacted);
                        }
                    }
                }
            }

            return LoggingJsonSerializer.Serialize(responseAsJson);
        }
        catch
        {
            return responseBodyText;
        }
    }
    public static IDictionary<string, JsonNode> FilterDictionary(IDictionary<string, JsonNode> data)
    {
        var keys = data.Keys.ToList();
        foreach (var key in keys)
        {
            if (_redactKeys!.Contains(key))
            {
                data[key] = "***";
                continue;
            }
            if (data[key] is not null)
            {
                if (data[key].GetValueKind() == JsonValueKind.Object)
                {
                    var innerDict = data[key] as IDictionary<string, JsonNode>;
                    if (innerDict != null)
                    {
                        var decResult = FilterDictionary(innerDict);
                        data[key] = decResult as JsonObject;
                    }
                }
                else if (data[key].GetValueKind() == JsonValueKind.String)
                {
                    data[key] = FilterString(key, data[key].ToString());
                }
            }
        }
        return data;
    }
    static string FilterString(string key, string textInResponse)
    {
        if (_redactKeys!.Contains(key))
        {
            return "***";
        }
        return textInResponse;
    }
}



