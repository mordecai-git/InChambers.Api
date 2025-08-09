using InChambers.Core.Models.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Results;

namespace InChambers.Core.Extensions;

public class CustomResultFactory : IFluentValidationAutoValidationResultFactory
{
    public IActionResult CreateActionResult(ActionExecutingContext context, ValidationProblemDetails validationProblemDetails)
    {
        var errorResponse = new ErrorResult("Validation Errors", "")
        {
            ValidationErrors = validationProblemDetails?.Errors
        };

        return new BadRequestObjectResult(errorResponse);
    }
}