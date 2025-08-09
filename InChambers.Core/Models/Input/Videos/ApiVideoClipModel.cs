using FluentValidation;

namespace InChambers.Core.Models.Input.Videos;

public class ApiVideoClipModel
{
    public int startTimecode { get; set; }
    public int endTimecode { get; set; }
}

public class ApiVideoClipValidator : AbstractValidator<ApiVideoClipModel>
{
    public ApiVideoClipValidator()
    {
        RuleFor(x => x.startTimecode)
            .LessThan(x => x.endTimecode);

        RuleFor(x => x.endTimecode)
            .NotEqual(0)
            .GreaterThan(x => x.startTimecode);
    }
}