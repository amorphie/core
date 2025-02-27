using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace amorphie.core.test.Caching;

public class CacheMiddlewareTests
{
    private static readonly Regex RoutePatternRegex = new Regex(@"\{(\w+)\}", RegexOptions.Compiled);
    [Fact]
    public void GenerateCacheKey_Should_Generate_Correct_Key_Without_Format()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/customers";
        context.Request.QueryString = new QueryString("?id=123&name=John");
        context.Request.Headers["X-Request-Id"] = "abc-123";

        var headersToDiffer = new List<string> { "X-Request-Id" };

        // Act
        var cacheKey = GenerateCacheKey(context.Request, headersToDiffer);

        // Assert
        Assert.Equal("/api/customers|id-123|name-John|X-Request-Id-abc-123", cacheKey);
    }

    [Fact]
    public void GenerateCacheKey_Should_Generate_Correct_Key_With_CacheFormat()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/domain/456";
        context.Request.QueryString = new QueryString("?user=789");
        context.Request.Headers["X-Correlation-Id"] = "xyz-456";
        context.Request.RouteValues["id"] = "456";
        var headersToDiffer = new List<string> { "X-Correlation-Id" };
        var cacheFormat = "/domain/{id}/user/{user}";

        // Act
        var cacheKey = GenerateCacheKey(context.Request, headersToDiffer, cacheFormat: cacheFormat);

        // Assert
        Assert.Equal("/domain/456/user/789", cacheKey);
    }

    [Fact]
    public void GenerateCacheKey_Should_Keep_Unresolved_Variables()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/domain";
        context.Request.QueryString = new QueryString("?user=789");

        var headersToDiffer = new List<string> { "X-Correlation-Id" };
        var cacheFormat = "/domain/{id}/user/{user}";

        // Act
        var cacheKey = GenerateCacheKey(context.Request, headersToDiffer, cacheFormat: cacheFormat);

        // Assert
        Assert.Equal("/domain/{id}/user/789", cacheKey); // {id} çözülemediği için olduğu gibi kalmalı
    }

    [Fact]
    public void GenerateCacheKey_Should_Handle_Empty_Query_And_Headers()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/orders";

        var headersToDiffer = new List<string> { "X-Request-Id" };

        // Act
        var cacheKey = GenerateCacheKey(context.Request, headersToDiffer);

        // Assert
        Assert.Equal("/api/orders", cacheKey); // Query ve Header olmadığı için sadece Path kullanılmalı
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
            // Önce match öncesindeki kısmı ekle
            resultBuilder.Append(cacheFormat, lastIndex, match.Index - lastIndex);

            var key = match.Groups[1].Value;
            if (valueDictionary.TryGetValue(key, out var val))
            {
                resultBuilder.Append(val); // Değer varsa ekle
            }
            else
            {
                resultBuilder.Append(match.Value); // Değer yoksa aynı bırak
            }

            lastIndex = match.Index + match.Length;
        }

        // Son kalan kısmı ekleyelim
        resultBuilder.Append(cacheFormat, lastIndex, cacheFormat.Length - lastIndex);

        return resultBuilder.ToString();
    }
}