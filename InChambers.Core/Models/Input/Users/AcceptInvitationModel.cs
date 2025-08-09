using FluentValidation;

namespace InChambers.Core.Models.Input.Users;

public class AcceptInvitationModel
{
    public required string Email { get; set; }
    public required string Token { get; set; }
    public required string Password { get; set; }
    public required string ConfirmPassword { get; set; }
}

public class AcceptInvitationValidator : AbstractValidator<AcceptInvitationModel>
{
    public AcceptInvitationValidator()
    {
        RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required");
        RuleFor(x => x.Token).NotEmpty().WithMessage("Token is required");
        RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required");
        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password).WithMessage("Passwords do not match");
    }
}