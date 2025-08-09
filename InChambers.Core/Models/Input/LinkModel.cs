using FluentValidation;

namespace InChambers.Core.Models.Input;

public class LinkModel
{
    public required string Title { get; set; }
    public required string Link { get; set; }
}

public class LinkValidator : AbstractValidator<LinkModel>
{
    public LinkValidator()
    {
        RuleFor(model => model.Title)
            .NotEmpty().WithMessage("Title cannot be empty.")
            .MaximumLength(150).WithMessage("Title cannot exceed 150 characters.");

        RuleFor(model => model.Link)
            .NotEmpty().WithMessage("Link cannot be empty.")
            .MaximumLength(255).WithMessage("Link cannot exceed 255 characters.");
    }
}