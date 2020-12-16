using System;
using API.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace API.Controllers
{
    public class User
    {
        public int Age { get; set; }
        public string Name { get; set; }
    }

    [ApiController]    
    [Route("[controller]")]    
    public class WeatherForecastController : APIControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Add([FromBody] User user)
        {
            return Ok();
        }

        [HttpGet]   
        [Authorize]
        public IActionResult Get()
        {
            //return new ObjectResult(new ProblemDetails
            //{
            //    Detail = "test",
            //    Instance = "test",
            //    Status = 401,
            //    Title = "title",
            //    Type = "test"
            //})
            //{
            //    StatusCode = 404,
            //    ContentTypes = 
            //    {
            //        new MediaTypeHeaderValue(new Microsoft.Extensions.Primitives.StringSegment("application/problem+json")),
            //    }
            //};

            throw new TestException("API Test Exception");
            //throw new Exception("Test");           


            var rng = new Random();
            //return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            //{
            //    Date = DateTime.Now.AddDays(index),
            //    TemperatureC = rng.Next(-20, 55),
            //    Summary = Summaries[rng.Next(Summaries.Length)]
            //})
            //.ToArray();
        }
    }

    public class TestException : Exception, IOdmWebApiException
    {
        public TestException(string? message) : base(message)
        {

        } 

        public string Type => "test";

        public string Title => "test";

        public int Status => 403;

        public string Detail => "test";

        public string Instance => "test";
    }
}
