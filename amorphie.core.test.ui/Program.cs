using amorphie.core.Extension;
using System.Reflection;
using amorphie.core.Middleware.Logging;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.AddSeriLogWithHttpLogging<AmorphieLogEnricher>();

var app = builder.Build();

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
    logger.LogCritical("Critical Log Message");

    var forecast = Enumerable.Range(1, 5).Select(index =>
       new WeatherForecast
       (
           Random.Shared.Next(-20, 55),
           summaries[Random.Shared.Next(summaries.Length)]
       ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.MapPost("/forecast1", ([FromBody] WeatherForecast weatherForecast) =>
{
    return "Test";
})
.WithName("PostForecast1")
.WithOpenApi();


app.UseLoggingHandlerMiddlewares();
app.Run();

record WeatherForecast(int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

