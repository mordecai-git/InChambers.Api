using FluentValidation;

namespace InChambers.Core.Models.Input.Auth;

public class ResetPasswordModel
{
    public required string Token { get; set; }
    public required string Email { get; set; }
    public required string NewPassword { get; set; }
    public required string ConfirmPassword { get; set; }
}

public class RestPasswordValidator : AbstractValidator<ResetPasswordModel>
{
    public RestPasswordValidator()
    {
        RuleFor(x => x.Token).NotEmpty().WithMessage("Token is required");
        RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required");
        RuleFor(x => x.NewPassword).NotEmpty().WithMessage("New password is required");
        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.NewPassword).WithMessage("Passwords do not match");
    }
}