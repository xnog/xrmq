using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using X;

namespace Api.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;
    private readonly IXrmq xrmq;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, IXrmq xrmq)
    {
        _logger = logger;
        this.xrmq = xrmq;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        xrmq.Publish<Message>(string.Empty, "teste", null!, new Message {
            Code = "code",
            Name = "name",
        });

        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Process.GetCurrentProcess().Threads.Count,
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }
}
