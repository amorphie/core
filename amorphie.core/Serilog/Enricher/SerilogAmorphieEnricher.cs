using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog.Core;
using Serilog.Events;
using System.Reflection;

namespace amorphie.core.Serilog.Enricher;

public class SerilogAmorphieEnricher : ILogEventEnricher
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    private readonly string[] wild = { "Authorization", "Password" };
    public SerilogAmorphieEnricher() : this(new HttpContextAccessor())
    {
    }

    public SerilogAmorphieEnricher(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private LogEvent _logEvent;
    private ILogEventPropertyFactory _propertyFactory;

    public async void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        _logEvent = logEvent;
        _propertyFactory = propertyFactory;


        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext is not null)
        {

            try
            {
                foreach (var header in httpContext.Request.Headers)
                {
                    AddPropertyIfAbsent($"header.{header.Key}", header.Value);
                }
                foreach (var query in httpContext.Request.Query)
                {
                    AddPropertyIfAbsent($"query.{query.Key}", query.Value);
                }

                var request = httpContext.Request;
                var stream = request.Body;// At the begining it holding original request stream                    
                var originalReader = new StreamReader(stream);
                var originalContent = await originalReader.ReadToEndAsync();

                JObject jsonObject = JsonConvert.DeserializeObject<JObject>(originalContent);
                if (jsonObject != null)
                    RecursiveJsonLoop(jsonObject, "body");
            }
            catch (Exception ex)
            {

            }
        }

        AddPropertyIfAbsent("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));
        AddPropertyIfAbsent("ApplicationName", Assembly.GetExecutingAssembly().GetName().Name);
    }
    void RecursiveJsonLoop(JObject jsonObject, string currentPath)
    {
        if (jsonObject == null)
            return;

        foreach (var property in jsonObject.Properties())
        {
            string newPath = currentPath == "" ? property.Name : $"{currentPath}.{property.Name}";

            if (property.Value.Type == JTokenType.Object)
            {
                RecursiveJsonLoop((JObject)property.Value, newPath);
            }
            else if (property.Value.Type == JTokenType.Array)
            {
                for (int i = 0; i < ((JArray)property.Value).Count; i++)
                {
                    RecursiveJsonLoop((JObject)((JArray)property.Value)[i], $"{newPath}[{i}]");
                }
            }
            else
            {
                AddPropertyIfAbsent($"{newPath}", property.Value.ToString());
            }
        }
    }

    void AddPropertyIfAbsent(string key, object value)
    {
        if (wild.Contains(key))
            value = "******";

        _logEvent.AddPropertyIfAbsent(_propertyFactory.CreateProperty(key, value, true));
    }
}

