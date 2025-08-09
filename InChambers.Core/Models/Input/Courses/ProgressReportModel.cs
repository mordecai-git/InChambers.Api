using FluentValidation;

namespace InChambers.Core.Models.Input.Courses;

public class ProgressReportModel
{
    public decimal Progress { get; set; }
}

public class ProgressReportValidator : AbstractValidator<ProgressReportModel>
{
    public ProgressReportValidator()
    {
        RuleFor(x => x.Progress).GreaterThanOrEqualTo(0);
    }
}