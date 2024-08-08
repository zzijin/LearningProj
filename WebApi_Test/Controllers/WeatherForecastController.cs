using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Net;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WebApi_Test.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class WeatherForecastController : ControllerBase
    {
        private string[] summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger,IMemoryCache memory)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = summaries[Random.Shared.Next(summaries.Length)]
            })
            .ToArray();
        }

        [HttpGet(Name = "GetFile")]
        public async Task<ActionResult<byte[]>> GetFile(string path)
        {
            try
            {

                using (var fs = new FileStream(path, FileMode.Open))
                {
                    byte[] data = new byte[fs.Length];
                    await Task.Delay(5000);
                    await fs.ReadAsync(data, 0, data.Length);
                    return data;
                }
            }
            catch (Exception ex)
            {
                return NotFound(ex);
            }
        }

        [HttpPut(Name = @"PutSummaries")]
        public HttpStatusCode PutSummaries(string[] array)
        {
            summaries = array;
            return HttpStatusCode.OK;
        }

    }
}
