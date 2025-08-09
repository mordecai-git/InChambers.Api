using InChambers.Core.Interfaces;
using InChambers.Core.Models.Input.Orders;
using InChambers.Core.Models.Utilities;
using InChambers.Core.Models.View.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InChambers.Api.Controllers;

[ApiController]
[Route("api/v1/orders")]
[Authorize]
public class OrderController : BaseController
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
    }

    /// <summary>
    /// Validates a discount code.
    /// </summary>
    /// <remarks>
    /// This endpoint validates a discount code and returns the discount information. <br/>
    /// Requires authentication.
    /// </remarks>
    /// <param name="discountCode">The discount code to validate.</param>
    /// <param name="totalAmount">The total amount of the order.</param>
    /// <response code="200">Returns the discount information.</response>
    /// <response code="400">Returns an error if any occurred.</response>
    [HttpGet("validate-discount/{discountCode}/{totalAmount}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResult<DiscountView>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
    public async Task<IActionResult> ValidateDiscount(string discountCode, decimal totalAmount)
    {
        var result = await _orderService.ValidateDiscount(discountCode, totalAmount);
        return ProcessResponse(result);
    }

    /// <summary>
    /// Creates a new order.
    /// </summary>
    /// <remarks>
    /// This endpoint creates a new order with the specified details. <br/>
    /// Requires authentication.
    /// </remarks>
    /// <param name="model">The details of the order to create.</param>
    /// <response code="201">Returns the payment information.</response>
    /// <response code="400">Returns an error if any occurred.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(SuccessResult<PaymentRequestView>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
    public async Task<IActionResult> CreateOrder([FromBody] NewOrderModel model)
    {
        var result = await _orderService.PlaceOrder(model);
        return ProcessResponse(result);
    }

    /// <summary>
    /// Attempts to retry payment for an order.
    /// </summary>
    /// <remarks>
    /// This endpoint attempts to retry payment for an order with the specified identifier. <br/>
    /// Requires authentication.
    /// </remarks>
    /// <param name="orderId">The identifier of the order for which payment is to be retried.</param>
    /// <response code="200">Returns the payment information.</response>
    /// <response code="404">Returns Not Found if the order for which payment should be retried does not exist.</response>
    [HttpGet("{orderId}/retry-payment")]
    [ProducesResponseType(typeof(SuccessResult<PaymentRequestView>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFoundErrorResult), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AttemptPayment(int orderId)
    {
        var res = await _orderService.AttemptPayment(orderId);
        return ProcessResponse(res);
    }

    /// <summary>
    /// Confirms payment for an order.
    /// </summary>
    /// <remarks>
    /// This endpoint confirms payment for an order with the specified identifier. <br/>
    /// Requires authentication.
    /// </remarks>
    /// <param name="orderId">The identifier of the order for which payment is to be confirmed.</param>
    /// <response code="200">Returns a success message</response>
    /// <response code="404">Returns Not Found if order for which payment should be confirmed does not exist.</response>
    /// <response code="400">Returns an error if any occurred.</response>
    [HttpGet("{orderId}/confirm-payment")]
    [ProducesResponseType(typeof(SuccessResult<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFoundErrorResult), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmPayment(int orderId)
    {
        var result = await _orderService.ConfirmPayment(orderId);
        return ProcessResponse(result);
    }
}
