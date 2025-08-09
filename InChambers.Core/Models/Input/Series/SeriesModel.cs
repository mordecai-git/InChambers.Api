using FluentValidation;

namespace InChambers.Core.Models.Input.Series;

public class SeriesModel
{
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Summary { get; set; } = null!;
    public List<string> Tags { get; set; } = new();
    public List<PriceModel> Prices { get; set; } = new();
    public List<LinkModel> UsefulLinks { get; set; } = new();
}

public class SeriesModelValidator : AbstractValidator<SeriesModel>
{
    public SeriesModelValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title cannot be empty.")
            .MaximumLength(100).WithMessage("Title cannot exceed 100 characters.");
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description cannot be empty.");
        RuleFor(x => x.Prices)
            .NotEmpty().WithMessage("Prices cannot be empty.");

        RuleForEach(x => x.Prices).SetValidator(new PriceValidation());

        RuleForEach(x => x.UsefulLinks).SetValidator(new LinkValidator());

        RuleForEach(x => x.Tags)
            .MaximumLength(20).WithMessage("Tag cannot exceed 20 characters.");
    }
}