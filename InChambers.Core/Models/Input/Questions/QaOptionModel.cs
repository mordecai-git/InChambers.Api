using FluentValidation;

namespace InChambers.Core.Models.Input.Questions;

public class QuestionOptionModel
{
    public int Id { get; set; }
    public string Value { get; set; } = null!;
}

public class QuestionOptionValidator : AbstractValidator<QuestionOptionModel>
{
    public QuestionOptionValidator()
    {
        RuleFor(x => x.Value).NotEmpty().MaximumLength(255);
    }
}