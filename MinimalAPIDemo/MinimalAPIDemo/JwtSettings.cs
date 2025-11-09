namespace MinimalAPIDemo;

public class JwtSettings
{
    public record JwtSetting(string Key, string Issuer, string Audience);
}
