using System.Net;
using System.Text.Json.Nodes;
using System.Text;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Routing;
namespace amorphie.core.Middleware.Logging;
public class LoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger _logger;
    private readonly LoggingOptions _loggingOptions;
    private const string _logTemplate = "RequestMethod : {RequestMethod}, RequestBody : {RequestBody}, RequestHost : {RequestHost}, RequestHeader : {RequestHeader}, ResponseBody : {ResponseBody}, ResponseStatus : {ResponseStatus}, ElapsedTime : {ElapsedTime}, RequestLength : {RequestLength}, ResponseLength : {ResponseLength}";
    public LoggingMiddleware(RequestDelegate next, ILoggerFactory loggerFactory, LoggingOptions loggingOptions)
    {
        _next = next;
        _logger = loggerFactory.CreateLogger<LoggingMiddleware>();
        _loggingOptions = loggingOptions;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        Stream? originalResponseBody = null;
        JsonObject? requestHeaders = null;
        string? requestBody = null;
        string? responseBody = null;
        var stopWatch = Stopwatch.StartNew();
        try
        {
            if (!context.Request.Path.HasValue)
            {
                await _next(context);
            }
            //if path is ignored do not log
            else if (_loggingOptions.IgnorePaths != null && Array.Exists(_loggingOptions.IgnorePaths, context.Request.Path.Value.Contains))
            {
                await _next(context);
            }
            else
            {
                var routeOption = FindRouteOptionForRequestPath(context.Request.Path.Value);
                context.Request.Headers.TryGetValue(LoggingConstants.Headers.XWorkflowName, out var wfName);
                var ignoreByWfName = _loggingOptions.IgnoreContentByWorkflowName?.Contains(wfName.ToString()) == true;

                requestHeaders = LogRequestHeaders(context, routeOption);

                requestBody = ignoreByWfName || context.Request.Method == HttpMethod.Get.Method ? "" : await LogRequestBodyAsync(context.Request, routeOption);

                if ((routeOption.LogAll || routeOption.LogResponse) && !ignoreByWfName)
                {
                    //Buffer response body
                    using var newResponseBody = new MemoryStream();
                    originalResponseBody = context.Response.Body;
                    context.Response.Body = newResponseBody;
                    responseBody = await InvokeInternalAsync(context, originalResponseBody, newResponseBody, routeOption);
                }
                else
                {
                    await _next(context);
                }
                var headers = LogResponseHeaders(context, requestHeaders);
                stopWatch.Stop();
                var responseLength = responseBody == null ? 0 : System.Text.ASCIIEncoding.Unicode.GetByteCount(responseBody);
                _logger.LogInformation(_logTemplate,
                    context.Request.Method,
                    requestBody,
                    context.Request.Host,
                    headers,
                    responseBody,
                    context.Response.StatusCode,
                    stopWatch.ElapsedMilliseconds,
                    GetByteSize(requestBody),
                    GetByteSize(responseBody)
                    );
            }
        }
        catch (Exception ex)
        {
            if (originalResponseBody != null)
            {
                context.Response.Body = originalResponseBody;
            }
            stopWatch.Stop();

            await HandleExceptionAsync(context, ex, requestHeaders, requestBody, responseBody, stopWatch.ElapsedMilliseconds);
        }
    }

    private int GetByteSize(string? s)
    {
        return s == null ? 0 : ASCIIEncoding.Unicode.GetByteCount(s);
    }


    private async Task<string?> InvokeInternalAsync(HttpContext context, Stream originalResponseBody, MemoryStream newResponseBody, LoggingRouteOptions routeOptions)
    {
        await _next(context);
        //Read response body
        newResponseBody.Seek(0, SeekOrigin.Begin);
        var responseBodyText = await new StreamReader(newResponseBody).ReadToEndAsync();

        //Rewind and set to originalResponseBody
        newResponseBody.Seek(0, SeekOrigin.Begin);
        await newResponseBody.CopyToAsync(originalResponseBody);

        return LogResponseBody(responseBodyText, routeOptions);
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex, JsonObject? requestHeaders, string? requestBody, string? responseBody, long elapsedTime)
    {
        var errorMessage = "An error occured and logged. Use trace identifier id to find out details";
        var errorDto = new ErrorModel
        {
            ErrorMessage = errorMessage,
            TraceIdentifier = context.TraceIdentifier,
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = ex is BadHttpRequestException badEx ? badEx.StatusCode : (int)HttpStatusCode.InternalServerError;
        _logger.LogError(ex, _logTemplate,
            context.Request.Method,
            requestBody,
            context.Request.Host,
            requestHeaders?.ToJsonString(),
            responseBody,
            context.Response.StatusCode,
            elapsedTime,
            GetByteSize(requestBody),
            GetByteSize(responseBody)
            );
        await context.Response.WriteAsync(LoggingJsonSerializer.Serialize(errorDto));
    }

    private JsonObject LogRequestHeaders(HttpContext httpContext, LoggingRouteOptions routeOptions)
    {
        var requestHeaders = new JsonObject();
        if (routeOptions.LogAll || routeOptions.LogHeader)
        {
            foreach (var pair in httpContext.Request.Headers)
            {
                if (routeOptions.IgnoreFields?.Contains(pair.Key.ToLower()) == true)
                {
                    requestHeaders.Add(pair.Key, "***");
                }
                else
                {
                    requestHeaders.Add(pair.Key, pair.Value.ToString().Replace("\"", ""));
                }
            }
        }
        return requestHeaders;
    }
    private async ValueTask<string> LogRequestBodyAsync(HttpRequest request, LoggingRouteOptions routeOptions, Encoding? encoding = null)
    {
        if (routeOptions.LogAll || routeOptions.LogRequest)
        {
            request.EnableBuffering();
            using var reader = new StreamReader(request.Body, encoding ?? Encoding.UTF8, leaveOpen: true);
            string body = await reader.ReadToEndAsync();
            body = body.Replace("\n", "").Replace("\r", "").Replace(" ", "");
            if (routeOptions.IgnoreFields?.Length > 0)
            {
                body = LoggingHelper.FilterContent(body, routeOptions);
            }
            request.Body.Position = 0;

            return body;
        }
        return "";
    }
    private string LogResponseBody(string responseBodyText, LoggingRouteOptions routeOptions)
    {
        if (routeOptions.IgnoreFields?.Length > 0)
        {
            responseBodyText = LoggingHelper.FilterContent(responseBodyText, routeOptions);
        }
        return responseBodyText;
    }
    private string LogResponseHeaders(HttpContext httpContext, JsonObject headers)
    {
        if (httpContext.Response.Headers.FirstOrDefault(p => p.Key == "FromCache").Key != null)
        {
            headers.TryAdd("ResponseHeader_FromCache", "true");
        }
        return headers.ToJsonString();
    }
    /// <summary>
    /// //if option not found for specified route returns default
    /// </summary>
    /// <param name="requestPath"></param>
    /// <returns></returns>
    private LoggingRouteOptions FindRouteOptionForRequestPath(string requestPath)
    {
        if (_loggingOptions.Routes != null)
        {
            foreach (var routeOption in _loggingOptions.Routes)
            {
                if (Regex.IsMatch(requestPath, routeOption.Regex))
                {
                    if (routeOption.IgnoreFields == null)
                    {
                        routeOption.IgnoreFields = _loggingOptions.Default.IgnoreFields;
                    }
                    if (routeOption.LogFields == null)
                    {
                        routeOption.LogFields = _loggingOptions.Default.LogFields;
                    }
                    _logger.LogDebug("Logging route option found for path: {RequestPath}, Regex: {Regex}", requestPath, routeOption.Regex);
                    return routeOption;
                }
            }
        }
        _logger.LogDebug("Logging route option NOT found for path: {RequestPath}. Default option is taken", requestPath);
        return _loggingOptions.Default;
    }

    private class ErrorModel
    {
        public string? ErrorMessage { get; set; }
        public string? TraceIdentifier { get; set; }
    }
}