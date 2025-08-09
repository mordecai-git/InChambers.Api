using FluentValidation;

namespace InChambers.Core.Models.Input.Users;

public class UserInvitationModel
{
    public required string Email { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Phone { get; set; }
    public bool CanManageCourses { get; set; }
    public bool CanManageUsers { get; set; }
}

public class UserInvitationValidator : AbstractValidator<UserInvitationModel>
{
    public UserInvitationValidator()
    {
        RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required");
        RuleFor(x => x.FirstName).NotEmpty().WithMessage("First name is required");
        RuleFor(x => x.LastName).NotEmpty().WithMessage("Last name is required");
        RuleFor(x => x.Phone).NotEmpty().WithMessage("Phone is required");
    }
}