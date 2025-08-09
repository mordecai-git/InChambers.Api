using FluentValidation;

namespace InChambers.Core.Models.Input.Auth;

public class ConfirmEmailModel
{
    public required string Email { get; set; }
    public required string Token { get; set; }
}

public class ConfirmEmailValidator : AbstractValidator<ConfirmEmailModel>
{
    public ConfirmEmailValidator()
    {
        RuleFor(x => x.Email).EmailAddress();
        RuleFor(x => x.Token).NotEmpty();
    }
}