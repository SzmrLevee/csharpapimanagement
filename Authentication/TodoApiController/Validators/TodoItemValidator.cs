using FluentValidation;
using TodoApiController.Model;

namespace TodoApiController.Validators
{
    //FluentValidatort fel kell rakni a nuget packagesnél!
    public class TodoItemValidator : AbstractValidator<TodoItem>
    {
       public TodoItemValidator()
        {
            RuleFor(x => x.Title).Length(10, 200);
            RuleFor(x => x.Description).NotEmpty();
        }
    }
}
