using amorphie.core.Extension;
using System.Reflection;
using amorphie.core.Middleware;
using amorphie.core.Middleware.Logging;
using Microsoft.AspNetCore.Mvc;
using amorphie.workflow.core.test.ui;
using Refit;
using StackExchange.Redis;
using Amorphie.Core.Cache.Redis;
using amorphie.core.test.ui.Modules;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.AddSeriLog<TestLogEnricher>();

if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddElasticApm();
}
#region Cache
builder.AddDistributedCaching();

#endregion



var app = builder.Build();
app.UseLoggingHandlerMiddlewares();

app.UseDistributedCaching();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var summaries = new[]
{
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

app.MapGet("/weatherforecast", (ILogger<object> logger) =>
{
    //logger.LogTrace("Trace Log Message");
    //logger.LogDebug("Debug Log Message");
    //logger.LogInformation("Information Log Message");
    //logger.LogWarning("Warning Log Message");
    //logger.LogError("Error Log Message");
    //logger.LogCritical("Critical Log Message");

    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return new { forecast = forecast };
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.MapPost("/weatherforecast/{varr}", (ILogger<object> logger, [FromRoute(Name = "varr")] Guid varr) =>
{
    logger.LogTrace("Trace Log Message");
    logger.LogDebug("Debug Log Message");
    logger.LogInformation("Information Log Message");
    logger.LogWarning("Warning Log Message");
    logger.LogError("Error Log Message");
    logger.LogCritical("Critical Log Message");

    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return new { forecast = forecast };
})
.WithName("PostWeatherForecast")
.WithOpenApi();


app.MapGet("/cachetest", CacheModule.GetDefinitionBulkAsync);

app.MapGet("/cachetest2", CacheModule.GetDefinitionBulkAsync);

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

