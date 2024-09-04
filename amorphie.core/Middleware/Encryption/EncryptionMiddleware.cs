using amorphie.core.Constants;
using amorphie.core.Extension;
using amorphie.core.Helper;
using amorphie.service.Filters;
using System.Text;
using System.Text.Json.Nodes;

namespace amorphie.core.Middleware;

public class EncryptionMiddleware
{

    private readonly RequestDelegate _next;
    private readonly ILogger _logger;
    private const string _logTemplate = "Error while encryption skipping. RequestMethod : {RequestMethod}, RequestHost : {RequestHost}, ResponseStatus : {ResponseStatus}";
    private readonly string _schemaUrl;
    public EncryptionMiddleware(RequestDelegate next, ILoggerFactory loggerFactory, string schemaUrl)
    {
        _next = next;
        _logger = loggerFactory.CreateLogger<EncryptionMiddleware>();
        _schemaUrl = schemaUrl;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.Request.EnableBuffering();

        var jsonSchemaResult = await DecryptRequestBodyAsync(context);
        if (jsonSchemaResult == null)
        {
            context.Request.Body.Position = 0;
            await _next(context);
        }
        else
        {
            using var responseBodyStream = new MemoryStream();

            var bodyStream = context.Response.Body;
            try
            {
                context.Response.Body = responseBodyStream;

                await _next(context);

                responseBodyStream.Seek(0, SeekOrigin.Begin);
                var responseBody = await new StreamReader(responseBodyStream).ReadToEndAsync();

                var responseBodyAsJson = AmorphieJsonSerializer.Deserialize<JsonObject>(responseBody);
                var instanceId = responseBodyAsJson?[ZeebeVariableKeys.InstanceId]?.ToString();

                var lastTransitionName = responseBodyAsJson?[ZeebeVariableKeys.LastTransition]?.ToString().DeleteUnAllowedCharecters();
                var lastTransitionData = responseBodyAsJson?[$"TRX{lastTransitionName}"] as IDictionary<string, JsonNode>;

                if (!string.IsNullOrEmpty(instanceId) && lastTransitionData?["data"]?["entityData"] is JsonObject entityData)
                {
                    FilterHelper.EncryptDict(entityData, jsonSchemaResult.ActualProperties, instanceId);
                    var responseContent = JsonContent.Create(responseBodyAsJson);
                    using var newbodyStream = await responseContent.ReadAsStreamAsync();//modified stream
                    await newbodyStream.CopyToAsync(bodyStream);
                }
                else
                {
                    responseBodyStream.Seek(0, SeekOrigin.Begin);
                    await responseBodyStream.CopyToAsync(bodyStream);
                }
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
            finally
            {
                context.Response.Body = bodyStream;
            }
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        _logger.LogError(ex, _logTemplate,
            context.Request.Method,
            context.Request.Host,
            context.Response.StatusCode);
        await _next(context);
    }


    private async ValueTask<NJsonSchema.JsonSchema?> DecryptRequestBodyAsync(HttpContext context, Encoding? encoding = null)
    {
        var requestBody = context.Request.Body;
        using var reader = new StreamReader(requestBody, encoding ?? Encoding.UTF8, leaveOpen: true);
        string body = await reader.ReadToEndAsync();
        if (!AmorphieJsonSerializer.TryDeserialize(body, out JsonObject? bodyAsJson))
            return null;

        var instanceId = bodyAsJson?[ZeebeVariableKeys.InstanceId]?.ToString();
        var lastTransitionName = bodyAsJson?[ZeebeVariableKeys.LastTransition]?.ToString();
        if (string.IsNullOrEmpty(instanceId) || string.IsNullOrEmpty(lastTransitionName))
        {
            return null;
        }


        var jsonSchemaResult = await GetJsonSchemaAsync(lastTransitionName);
        if (jsonSchemaResult == null)
        {
            return null;
        }

        //var lastTransitionData = bodyAsJson![$"TRX{lastTransitionName.DeleteUnAllowedCharecters()}"] as IDictionary<string, JsonNode>;
        //if (lastTransitionData == null)
        //{
        //    return null;
        //}
        //var entityData = lastTransitionData["data"]?["entityData"] as JsonObject;
        //if (entityData == null)
        //{
        //    return null;
        //}
        //var decryptResult = AesHelper.DecryptJson(instanceId, entityData);

        //decrypt everything ??
        var decryptResult = AesHelper.DecryptJson(instanceId, bodyAsJson);


        if (decryptResult == null)
        {
            return null;
        }

        var requestContent = JsonContent.Create(bodyAsJson);
        requestBody = await requestContent.ReadAsStreamAsync();//modified stream

        context.Request.Body = requestBody;

        return jsonSchemaResult;

    }

    private async Task<NJsonSchema.JsonSchema?> GetJsonSchemaAsync(string subjectName)
    {
        try
        {
            using var clientHttp = new HttpClient();
            HttpResponseMessage response = await clientHttp.GetAsync(_schemaUrl + subjectName);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var responseBody = (await response.Content.ReadAsStringAsync()).Replace("\\", "");
                var theSchema = await NJsonSchema.JsonSchema.FromJsonAsync(responseBody);
                return theSchema;
            }
        }
        catch
        {
            return null;
        }
        return null;
    }

}
