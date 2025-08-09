using FluentValidation;
using InChambers.Core.Utilities;
using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;

namespace InChambers.Core.Models.Input.AnnotatedAgreements;

public class AnnotatedAgreementModel
{
    [JsonIgnore]
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Summary { get; set; }
    public required string Description { get; set; }
    public decimal Price { get; set; }
    public List<string> Tags { get; set; } = new();
    public required IFormFile File { get; set; }
}

public class AnnotatedAgreementValidation : AbstractValidator<AnnotatedAgreementModel>
{
    public AnnotatedAgreementValidation()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title cannot be empty.")
            .MaximumLength(100).WithMessage("Title cannot exceed 100 characters.");
        RuleFor(x => x.Summary)
            .NotEmpty().WithMessage("Summary cannot be empty.")
            .MaximumLength(200).WithMessage("Summary cannot exceed 250 characters.");
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description cannot be empty.");
        RuleFor(x => x.Price)
            .NotEmpty().WithMessage("Price cannot be empty.");
        RuleForEach(x => x.Tags)
            .MaximumLength(20).WithMessage("Tag cannot exceed 20 characters.");
        RuleFor(x => x.File)
            .Custom((file, context) =>
            {
                var validationResult = CustomFileValidator.HaveValidFile(file);
                if (!validationResult.IsValid)
                    context.AddFailure($"File: {validationResult.ErrorMessage}");
            });
    }
}