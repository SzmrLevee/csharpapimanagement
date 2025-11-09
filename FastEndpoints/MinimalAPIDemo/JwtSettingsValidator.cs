using FluentValidation;

namespace MinimalAPIDemo
{
    public class JwtSettingsValidator : AbstractValidator<JwtSettings>
    {
        public JwtSettingsValidator() 
        {
            RuleFor(x=>x.Key).Length(32);
        }
    }
}
