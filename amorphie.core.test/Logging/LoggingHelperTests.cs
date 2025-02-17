using amorphie.core.Middleware.Logging;
using System.Text.Json.Nodes;

namespace amorphie.core.test.Logging;
public class LoggingHelperTests
{
    [Fact]
    public void FilterContent_EmptyResponseBody_ReturnsEmptyString()
    {
        // Arrange
        var routeOptions = new LoggingRouteOptions();
        string responseBodyText = string.Empty;

        // Act
        var result = LoggingHelper.FilterContent(responseBodyText, routeOptions);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void FilterContent_ValidJsonResponse_ReturnsFilteredJson()
    {
        // Arrange
        var routeOptions = new LoggingRouteOptions
        {
            IgnoreFields = new string[] { "password" },
            LogFields = new string[] { "username" }
        };
        string responseBodyText = "{\"username\":\"user1\",\"password\":\"pass123\"}";

        // Act
        var result = LoggingHelper.FilterContent(responseBodyText, routeOptions);

        // Assert
        Assert.Contains("\"username\":\"user1\"", result);
        Assert.Contains("\"password\":\"***\"", result);
    }

    [Fact]
    public void FilterJson_ValidJsonObject_ReturnsFilteredJsonObject()
    {
        // Arrange
        var routeOptions = new LoggingRouteOptions
        {
            IgnoreFields = new string[] { "password" },
            LogFields = new string[] { "username" }
        };
        var jsonObject = JsonNode.Parse("{\"username\":\"user1\",\"password\":\"pass123\"}")!.AsObject();

        // Act
        var result = LoggingHelper.FilterJson(jsonObject);

        // Assert
        Assert.Equal("user1", result["username"]!.ToString());
        Assert.Equal("***", result["password"]!.ToString());
    }
}
