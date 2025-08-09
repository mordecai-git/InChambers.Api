using FluentValidation;

namespace InChambers.Core.Models.Input.Orders;

public class NewOrderModel
{
    public required string BillingAddress { get; set; }
    public int? DurationId { get; set; }
    public string DiscountCode { get; set; }
    public required string ItemUid { get; set; }
    public required string ItemType { get; set; }
    public required string CallBackUrl { get; set; }
}

public class NewOrderValidator : AbstractValidator<NewOrderModel>
{
    public NewOrderValidator()
    {
        RuleFor(x => x.BillingAddress).NotEmpty();
        RuleFor(x => x.ItemUid).NotEmpty();

        // Item type must be one of the Course, Series, SmeHub or AnnotatedAgreement
        RuleFor(x => x.ItemType).Must(x => x == "Course" || x == "Series" || x == "SmeHub" || x == "AnnotatedAgreement")
            .WithMessage("Invalid item type.");

        // Require DurationId when type is either Course or Series
        RuleFor(x => x.DurationId).NotEmpty().When(x => x.ItemType == "Course" || x.ItemType == "Series")
            .WithMessage("DurationId is required for Course or Series.");

        RuleFor(x => x.CallBackUrl).NotEmpty();
    }
}