using FluentValidation;
using InChambers.Core.Utilities;
using Microsoft.AspNetCore.Http;

namespace InChambers.Core.Models.Input;

public class FileUploadModel
{
    public required IFormFile File { get; set; }
}

public class FileUploadValidator : AbstractValidator<FileUploadModel>
{
    public FileUploadValidator()
    {
        RuleFor(x => x.File)
            .Custom((file, context) =>
            {
                var validationResult = CustomFileValidator.HaveValidFile(file);
                if (!validationResult.IsValid)
                    context.AddFailure($"File: {validationResult.ErrorMessage}");
            });
    }
}