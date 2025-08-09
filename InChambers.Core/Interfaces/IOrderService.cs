using InChambers.Core.Models.Input.Orders;
using InChambers.Core.Models.Utilities;

namespace InChambers.Core.Interfaces;

public interface IOrderService
{
    Task<Result> ValidateDiscount(string discountCode, decimal totalAmount);
    Task<Result> PlaceOrder(NewOrderModel model);
    Task<Result> AttemptPayment(int orderId);
    Task<Result> ConfirmPayment(int orderId);
}
