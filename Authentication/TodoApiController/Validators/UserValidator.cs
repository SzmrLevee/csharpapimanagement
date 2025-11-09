using FluentValidation;
using TodoApiController.Model;

namespace TodoApiController.Validators;

public class UserValidator : AbstractValidator<User>
{
    public UserValidator(IDataStore data) 
    {
        RuleFor(x => x.Email).EmailAddress();
        RuleFor(x => x.UserName).Length(5, 40);
        RuleFor(x => x.UserName)
            .Must(x => !((IItemStore<User>)data)
            .GetAll()
            .Any(y => y.UserName == x))
            .WithMessage("User already exists");
        RuleFor(x => x.UserName)
            .Must(x => x.All(y => char.IsAsciiLetterOrDigit(y)))
            .WithMessage("Must only contain ascii alphanumeric characters");
    }
}
