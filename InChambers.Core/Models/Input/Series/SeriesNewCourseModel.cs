using FluentValidation;
using InChambers.Core.Models.Input.Videos;

namespace InChambers.Core.Models.Input.Series;

public class SeriesNewCourseModel
{
    public required string Title { get; set; }
    public required string Summary { get; set; }
    public required VideoDetailModel VideoDetails { get; set; }
}

public class SeriesNewCourseModelValidator : AbstractValidator<SeriesNewCourseModel>
{
    public SeriesNewCourseModelValidator()
    {
        RuleFor(x => x.Title)
        .NotEmpty().WithMessage("Title cannot be empty.")
        .MaximumLength(100).WithMessage("Title cannot exceed 100 characters.");
        RuleFor(x => x.Summary)
            .NotEmpty().WithMessage("Summary cannot be empty.")
            .MaximumLength(200).WithMessage("Summary cannot exceed 250 characters.");
        RuleFor(x => x.VideoDetails).SetValidator(new VideoDetailValidator());
    }
}
