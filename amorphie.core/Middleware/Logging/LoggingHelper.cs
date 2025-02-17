using System.Text.Json.Nodes;
using System.Text.Json;
using Microsoft.AspNetCore.Routing;

namespace amorphie.core.Middleware.Logging;

public static class LoggingHelper
{
    private static LoggingRouteOptions? _routeOptions;
    const int maxDepth = 24;

    public static string FilterContent(string responseBodyText, LoggingRouteOptions routeOptions)
    {
        if (string.IsNullOrEmpty(responseBodyText))
        {
            return string.Empty;
        }
        _routeOptions = routeOptions;
        try
        {
            var responseAsJson = LoggingJsonSerializer.Deserialize<JsonObject>(responseBodyText);
            if (responseAsJson == null || (routeOptions.IgnoreFields?.Length == 0 && routeOptions.LogFields?.Length == 0))
            {
                return responseBodyText;
            }
            return LoggingJsonSerializer.Serialize(FilterJson(responseAsJson));
        }
        catch
        {
            return responseBodyText;
        }
    }
    public static JsonObject FilterJson(JsonObject responseAsJson, int depth = 0)
    {
        if (depth > maxDepth)
        {
            return responseAsJson;
        }
        var keys = responseAsJson.Select(p => p.Key).ToList();
        foreach (var key in keys)
        {
            if (_routeOptions!.IgnoreFields?.Contains(key) == true)
            {
                responseAsJson[key] = "***";
                continue;
            }
            if (_routeOptions!.LogFields?.Contains(key) == false)
            {
                responseAsJson[key] = "***";
                continue;
            }
            if (responseAsJson[key] is not null)
            {
                var valueKind = responseAsJson[key]!.GetValueKind();
                if (valueKind == JsonValueKind.Object)
                {
                    var innerDict = responseAsJson[key] as JsonObject;
                    if (innerDict != null)
                    {
                        depth++;
                        var decResult = FilterJson(innerDict, depth);
                        responseAsJson[key] = decResult;
                    }
                }
                else if (valueKind == JsonValueKind.String || valueKind == JsonValueKind.Number)
                {
                    responseAsJson[key] = FilterString(key, responseAsJson[key]!.ToString());
                }
            }
        }
        return responseAsJson;
    }
    static string FilterString(string key, string textInResponse)
    {
        if (_routeOptions!.IgnoreFields?.Contains(key) == true)
        {
            return "***";
        }
        if (_routeOptions!.LogFields?.Contains(key) == false)
        {
            return "***";
        }
        return textInResponse;
    }
}



