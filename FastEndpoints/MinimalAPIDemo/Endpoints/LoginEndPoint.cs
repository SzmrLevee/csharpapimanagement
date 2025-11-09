using FastEndpoints;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace MinimalAPIDemo.Endpoints
{
    public class LoginEndPoint : Endpoint<LoginEndPoint.LoginData, string>
    {
        private readonly JwtSettings jwt;

        public class LoginData
        {
            public string user { get; set; } = "";
            public string password { get; set; } = "";
        }

        public LoginEndPoint(IOptions<JwtSettings> options)
        {
            jwt = options.Value;
        }

        public override void Configure()
        {
            Post("/login");
            AllowAnonymous();
        }

        public override async Task HandleAsync(LoginEndPoint.LoginData req, CancellationToken ct)
        {
            if (req.user != req.password)
            {
                await SendAsync("Forbidden", statusCode: 403, cancellation: ct);
                return;
            }
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key));
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, req.user),
                new Claim(ClaimTypes.Role, "Finance"),
            };
            var token = new JwtSecurityToken(
                issuer: jwt.Issuer,
                audience: jwt.Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(15),
                signingCredentials: cred);
            var tokenStr = new JwtSecurityTokenHandler().WriteToken(token);
            await SendAsync(tokenStr, cancellation: ct);
        }
    }
}
