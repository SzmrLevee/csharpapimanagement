using FastEndpoints;
using System.Security.Claims;

namespace MinimalAPIDemo.Endpoints
{
    public class IdAlapjan : EndpointWithoutRequest<string>
    {
        public override void Configure()
        {
            Get("/id_alapjan/{id}");
            AllowAnonymous();
        }
        public override async Task HandleAsync(CancellationToken ct)
        {
            try
            {
                var identify = HttpContext.User.Identity as ClaimsIdentity;
                var user = identify!.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
                var id = Route<int>("id");
                if (id < 0 || id >= GetWeather.Summaries.Length)
                {
                    await SendAsync("Not found", statusCode: 404, cancellation: ct);
                    return;
                }
                await SendAsync($"user: " + user + " " + GetWeather.Summaries[id], cancellation: ct);
            }
            catch
            {
                await SendAsync("Not found", statusCode: 404, cancellation: ct);
            }
        }
    }
}
