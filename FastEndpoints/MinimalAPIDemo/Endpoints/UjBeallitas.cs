using FastEndpoints;
using FluentValidation;

namespace MinimalAPIDemo.Endpoints
{
    public class UjBeallitas : Endpoint<JwtSettings, string>
    {
        IValidator<JwtSettings> validator;

        public UjBeallitas(IValidator<JwtSettings> validator)
        {
            this.validator = validator;
        }

        public override void Configure()
        {
            Post("/uj_beallitas");
            AllowAnonymous();
        }
        public override async Task HandleAsync(JwtSettings req, CancellationToken ct)
        {
            var result = validator.Validate(req);
            if (!result.IsValid)
            {
                await SendAsync($"{String.Join(";", result.Errors.Select(x=>x.ErrorMessage))}", statusCode: 400, cancellation: ct);
                return;
            }
            await SendAsync($"{req.Issuer}", cancellation: ct);
        }
    }
}
