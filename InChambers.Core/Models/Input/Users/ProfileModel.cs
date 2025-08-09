using FluentValidation;

namespace InChambers.Core.Models.Input.Users;

public class ProfileModel
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Phone { get; set; }
}

public class ProfileValidator : AbstractValidator<ProfileModel>
{
    public ProfileValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(20);
    }
}