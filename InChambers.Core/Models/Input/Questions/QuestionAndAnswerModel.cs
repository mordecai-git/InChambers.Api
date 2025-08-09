using FluentValidation;

namespace InChambers.Core.Models.Input.Questions;

public class QuestionAndAnswerModel
{
    public int Id { get; set; }
    public required string CourseUid { get; set; }
    public string Text { get; set; } = null!;
    public bool IsMultiple { get; set; }
    public bool IsRequired { get; set; }

    public List<QuestionOptionModel> Options { get; set; } = new();
}

public class QuestionValidator : AbstractValidator<QuestionAndAnswerModel>
{
    public QuestionValidator()
    {
        RuleFor(x => x.Text).NotEmpty().MaximumLength(500);
        RuleForEach(x => x.Options).SetValidator(new QuestionOptionValidator());
    }
}