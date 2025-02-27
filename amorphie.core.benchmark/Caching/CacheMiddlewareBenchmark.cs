using System.Text;
using System.Text.RegularExpressions;
using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Http;

namespace amorphie.core.Caching;

/*
 *| Method                         | Mean     | Error   | StdDev  |
   |------------------------------- |---------:|--------:|--------:|
   | GenerateCacheKey_WithoutFormat | 162.0 ns | 0.58 ns | 0.54 ns |
   | GenerateCacheKey_WithFormat    | 375.9 ns | 4.15 ns | 3.47 ns |
 *
 */
public class CacheMiddlewareBenchmark
{
    private readonly HttpRequest _request;
    private readonly List<string> _headersToDiffer;
    private static readonly Regex RoutePatternRegex = new Regex(@"\{(\w+)\}", RegexOptions.Compiled);

    public CacheMiddlewareBenchmark()
    {
        var context = new DefaultHttpContext();
        _request = context.Request;

        // Örnek HTTP isteği oluşturuyoruz
        _request.Path = "/domain/456";
        _request.QueryString = new QueryString("?user=789");
        _request.Headers["X-Correlation-Id"] = "xyz-456";
        _request.RouteValues["id"] = "456";

        _headersToDiffer = new List<string> { "X-Correlation-Id" };
    }

    [Benchmark]
    public string GenerateCacheKey_WithoutFormat()
    {
        return GenerateCacheKey(_request, _headersToDiffer);
    }

    [Benchmark]
    public string GenerateCacheKey_WithFormat()
    {
        return GenerateCacheKey(_request, _headersToDiffer, cacheFormat: "/domain/{id}/user/{user}");
    }

    private static string GenerateCacheKey(HttpRequest request, List<string> headersToDiffer,
        string? overridePathWith = null, string? cacheFormat = null)
    {
        var keyBuilder = new StringBuilder();

        if (!string.IsNullOrEmpty(cacheFormat))
        {
            var cacheKey = BindCacheFormat(request, headersToDiffer, cacheFormat);
            keyBuilder.Append(cacheKey);
        }
        else
        {
            keyBuilder.Append(overridePathWith ?? request.Path);

            foreach (var (key, value) in request.Query.OrderBy(x => x.Key))
            {
                keyBuilder.Append($"|{key}-{value}");
            }

            foreach (var header in headersToDiffer)
            {
                if (request.Headers.TryGetValue(header, out var headerValue))
                {
                    keyBuilder.Append($"|{header}-{headerValue}");
                }
            }
        }

        return keyBuilder.ToString();
    }

    private static string BindCacheFormat(HttpRequest request, List<string> headersToDiffer, string cacheFormat)
    {
        var valueDictionary = new Dictionary<string, string>();

        foreach (var kvp in request.RouteValues)
        {
            if (kvp.Value is not null)
                valueDictionary[kvp.Key] = kvp.Value.ToString()!;
        }

        foreach (var kvp in request.Query)
        {
            valueDictionary[kvp.Key] = kvp.Value.ToString();
        }

        foreach (var header in headersToDiffer)
        {
            if (request.Headers.TryGetValue(header, out var headerValue))
            {
                valueDictionary[header] = headerValue.ToString();
            }
        }
        
        var resultBuilder = new StringBuilder();
        int lastIndex = 0;

        foreach (Match match in RoutePatternRegex.Matches(cacheFormat))
        {
            resultBuilder.Append(cacheFormat, lastIndex, match.Index - lastIndex);

            var key = match.Groups[1].Value;
            if (valueDictionary.TryGetValue(key, out var val))
            {
                resultBuilder.Append(val); 
            }
            else
            {
                resultBuilder.Append(match.Value);
            }

            lastIndex = match.Index + match.Length;
        }
        
        resultBuilder.Append(cacheFormat, lastIndex, cacheFormat.Length - lastIndex);

        return resultBuilder.ToString();
    }
}