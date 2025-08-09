using FluentValidation;

namespace InChambers.Core.Models.Input;

public class PriceModel
{
    public int DurationId { get; set; }
    public decimal Price { get; set; }
}

public class PriceValidation : AbstractValidator<PriceModel>
{
    public PriceValidation()
    {
        RuleFor(x => x.DurationId)
            .NotEmpty().WithMessage("Duration Id cannot be empty.");
        RuleFor(x => x.Price)
            .NotEmpty().WithMessage("Price cannot be empty.");
    }
}