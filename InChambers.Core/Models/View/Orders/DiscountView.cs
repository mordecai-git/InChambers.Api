namespace InChambers.Core.Models.View.Orders;

public class DiscountView
{
    public int Id { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Discount { get; set; }
    public decimal DiscountedAmount { get; set; }
    public string Message { get; set; } = null!;
}
