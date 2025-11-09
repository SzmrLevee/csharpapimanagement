using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;

namespace MinimalAPIDemo.Endpoints
{
    public class GetWeather : EndpointWithoutRequest<WeatherForecast[]>
    {

        public readonly static string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };
        public override void Configure()
        {
            Get("/weatherforecast");
            AllowAnonymous();
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var result = Enumerable.Range(1, 5).Select(index =>
                    new WeatherForecast
                    (
                        DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                        Random.Shared.Next(-20, 55),
                        Summaries[Random.Shared.Next(Summaries.Length)]
                    ))
                    .ToArray();
            await SendAsync(result, cancellation: ct);
        }
    }
}
