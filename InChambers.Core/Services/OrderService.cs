using Mapster;
using InChambers.Core.Constants;
using InChambers.Core.Interfaces;
using InChambers.Core.Models.App;
using InChambers.Core.Models.Configurations;
using InChambers.Core.Models.Input.Auth;
using InChambers.Core.Models.Input.Orders;
using InChambers.Core.Models.Paystack;
using InChambers.Core.Models.Utilities;
using InChambers.Core.Models.View.Orders;
using InChambers.Core.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace InChambers.Core.Services;

public class OrderService : IOrderService
{
    private readonly InChambersContext _context;
    private readonly UserSession _userSession;
    private readonly HttpClient _paystackClient;
    private readonly BaseURLs _baseUrls;

    public OrderService(InChambersContext context, UserSession userSession, IHttpClientFactory httpClientFactory, IOptions<PasystackConfig> paystackConfig, IOptions<AppConfig> appConfig)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _userSession = userSession ?? throw new ArgumentNullException(nameof(userSession));

        ArgumentException.ThrowIfNullOrEmpty(nameof(appConfig));
        ArgumentException.ThrowIfNullOrEmpty(nameof(paystackConfig));
        ArgumentException.ThrowIfNullOrEmpty(nameof(httpClientFactory));

        _paystackClient = httpClientFactory.CreateClient(paystackConfig.Value.HttpClientName);
        _baseUrls = appConfig.Value.BaseURLs;
    }

    public async Task<Result> ValidateDiscount(string discountCode, decimal totalAmount)
    {
        if (discountCode == null)
            return new ErrorResult(StatusCodes.Status400BadRequest, "Invalid discount code.");

        discountCode = discountCode.Trim();
        var discount = await _context.Discounts.FirstOrDefaultAsync(d => d.Code.Trim() == discountCode);

        if (discount == null)
            return new ErrorResult(StatusCodes.Status404NotFound, "Discount code not found");

        if (discount.TotalAvailable != -1 && discount.TotalUsed > discount.TotalAvailable)
            return new ErrorResult(StatusCodes.Status400BadRequest, "Discount code not available.");

        if (discount.IsSingleUse)
        {
            var alreadyUsed = await _context.Orders.AnyAsync(x => x.Discount!.Code.Trim() == discountCode && x.IsPaid);
            if (alreadyUsed)
                return new ErrorResult(StatusCodes.Status400BadRequest, "Discount code already used.");
        }

        bool isValid = discount.IsActive && !discount.IsDeleted && discount.ExpiryDate > DateTime.UtcNow;
        if (!isValid)
            return new ErrorResult(StatusCodes.Status404NotFound, "Discount code is not valid.");

        if (discount.MinAmount.HasValue && totalAmount < discount.MinAmount)
            return new ErrorResult(StatusCodes.Status400BadRequest, "Discount code is not valid for this amount.");

        var result = new DiscountView { Id = discount.Id, TotalAmount = totalAmount };

        // calculate discount
        if (discount.IsPercentage)
        {
            result.Discount = totalAmount * discount.Amount / 100;
            result.DiscountedAmount = totalAmount - result.Discount;
            result.Message = $"Discount of {discount.Amount}% applied, you have saved {result.Discount}";
        }
        else
        {
            result.Discount = discount.Amount;
            result.DiscountedAmount = totalAmount - result.Discount;
            result.Message = $"Discount applied, you have saved {result.Discount}";
        }

        return new SuccessResult(result);
    }

    public async Task<Result> PlaceOrder(NewOrderModel model)
    {
        var order = new Order
        {
            BillingAddress = model.BillingAddress,
            DurationId = model.DurationId,
            ItemType = model.ItemType,
            UserId = _userSession.UserId,
        };

        if (model.ItemType == OrderItemType.Course)
        {
            var course = await _context.Courses
              .Where(x => x.Uid == model.ItemUid)
              .Select(c => new Course
              {
                  Id = c.Id,
                  Prices = c.Prices
              }).FirstOrDefaultAsync();
            if (course == null)
                return new ErrorResult("Invalid course provided");

            order.CourseId = course.Id;

            // get course price
            var price = course.Prices.FirstOrDefault(x => x.DurationId == model.DurationId);
            if (price == null)
                return new ErrorResult("Invalid duration for course");

            order.ItemAmount = price.Price;

        }
        else if (model.ItemType == OrderItemType.Series)
        {
            var series = await _context.Series
              .Where(x => x.Uid == model.ItemUid)
              .Select(s => new Series
              {
                  Id = s.Id,
                  Prices = s.Prices
              }).FirstOrDefaultAsync();
            if (series == null)
                return new ErrorResult("Invalid series provided");

            order.SeriesId = series.Id;

            // get series price
            var price = series.Prices.FirstOrDefault(x => x.DurationId == model.DurationId);
            if (price == null)
                return new ErrorResult("Invalid duration for series");

            order.ItemAmount = price.Price;
        }
        else if (model.ItemType == OrderItemType.SmeHub)
        {
            var smeHub = await _context.SmeHubs
              .Where(x => x.Uid == model.ItemUid)
              .Select(s => new SmeHub
              {
                  Id = s.Id,
                  Price = s.Price
              }).FirstOrDefaultAsync();
            if (smeHub == null)
                return new ErrorResult("Invalid sme hub provided");

            order.SmeHubId = smeHub.Id;

            order.ItemAmount = smeHub.Price;
        }
        else if (model.ItemType == OrderItemType.AnnotatedAgreement)
        {
            var annotatedAgreement = await _context.AnnotatedAgreements
              .Where(x => x.Uid == model.ItemUid)
              .Select(a => new AnnotatedAgreement
              {
                  Id = a.Id,
                  Price = a.Price
              }).FirstOrDefaultAsync();
            if (annotatedAgreement == null)
                return new ErrorResult("Invalid annotated agreement provided");

            order.AnnotatedAgreementId = annotatedAgreement.Id;

            // get annotated agreement price
            order.ItemAmount = annotatedAgreement.Price;
        }
        else
        {
            return new ErrorResult("Invalid item selected.");
        }

        // attach discount
        order.TotalAmount = order.ItemAmount;

        if (!string.IsNullOrEmpty(model.DiscountCode))
        {
            var discountRes = await ValidateDiscount(model.DiscountCode, order.ItemAmount);
            if (discountRes is ErrorResult)
                return discountRes;

            var discount = (DiscountView)discountRes.Content;
            order.DiscountId = discount.Id;
            order.TotalAmount = order.ItemAmount - discount.Discount;
            order.DiscountApplied = discount.Discount;
        }

        await _context.AddAsync(order);
        await _context.SaveChangesAsync();

        var user = await _context.Users
            .Where(u => u.Id == _userSession.UserId)
            .Select(u => new
            {
                u.Email,
                u.FirstName,
                u.LastName
            }).FirstOrDefaultAsync();

        var transactionModel = new InitiateTransactionModel
        {
            email = user!.Email,
            // convert total amount to string and remove the decimal for paystack endpoint
            amount = order.TotalAmount.ToString("F").Replace(".", ""),
            metadata = JsonConvert.SerializeObject(user),
            callback_url = model.CallBackUrl + $"?orderId={order.Id}"
        };

        var initiatedTransaction = await InitiateTransaction(transactionModel);
        if (!initiatedTransaction.status) return new ErrorResult(initiatedTransaction.message);

        initiatedTransaction.data.Adapt(order);

        int saved = await _context.SaveChangesAsync();

        return saved > 0
            ? new SuccessResult(StatusCodes.Status201Created, order.Adapt<PaymentRequestView>())
            : new ErrorResult("An error occurred while placing the order");
    }

    public async Task<Result> AttemptPayment(int orderId)
    {
        var order = await _context.Orders
            .Where(x => x.Id == orderId)
            .Include(x => x.Discount)
            .FirstOrDefaultAsync();

        if (order is null)
            return new ErrorResult(StatusCodes.Status404NotFound, "Order not found.");

        if (order.IsPaid) return new ErrorResult("Payment has been completed.");

        // validate discount
        if (order!.DiscountId.HasValue)
        {
            var discountRes = await ValidateDiscount(order.Discount!.Code, order.ItemAmount);
            if (discountRes is ErrorResult)
                return new ErrorResult("Discount applied to order is no longer valid. Please try again.");
        }

        var paymentData = new PaymentRequestView
        {
            Authorization_Url = order.Authorization_Url!,
            Access_Code = order.Access_Code!,
            Reference = order.Reference!
        };

        return new SuccessResult(paymentData);
    }

    public async Task<Result> ConfirmPayment(int orderId)
    {
        var order = await _context.Orders
            .Where(x => x.Id == orderId)
            .Include(x => x.Duration)
            .Include(x => x.Discount)
            .FirstOrDefaultAsync();

        if (order == null) return new NotFoundErrorResult("Invalid order.");

        if (string.IsNullOrEmpty(order.Reference)) return new ErrorResult("Invalid payment confirmation attempt.");

        if (order.IsPaid) return new ErrorResult("Order payment is already completed.");

        var verifyTransaction = await VerifyTransaction(order.Reference!);

        if (!verifyTransaction.status) return new ErrorResult(verifyTransaction.message);
        if (!verifyTransaction.status || verifyTransaction.data.status != "success") return new ErrorResult("Payment not completed");

        var today = DateTime.UtcNow;
        order.IsPaid = true;
        order.PaidAt = today;
        order.UpdateById = _userSession.UserId;
        order.UpdatedAt = today;
        if (order.DiscountId.HasValue)
        {
            order.Discount!.TotalUsed++;
        }

        var userContent = new UserContent
        {
            UserId = order.UserId,
            StartDate = today,
            EndDate = today.AddMonths(order.Duration!.Count)
        };
        order.UserContent = userContent;
        _context.Orders.Update(order);

        // Add Course progress
        if (order.ItemType == OrderItemType.Course)
        {
            var userCourse = await _context.UserCourses
                .Where(x => x.UserId == order.UserId && x.CourseId == order.CourseId)
                .FirstOrDefaultAsync();

            if (userCourse != null)
            {
                userCourse.Progress = 0;
                userCourse.IsCompleted = false;
                userCourse.IsExpired = false;

                _context.UserCourses.Update(userCourse);
            }
            else
            {
                userCourse = new UserCourse
                {
                    UserId = order.UserId,
                    CourseId = order.CourseId!.Value,
                };
                await _context.AddAsync(userCourse);
            }
        }

        // Add Series progress
        else if (order.ItemType == OrderItemType.Series)
        {
            var userSeries = await _context.UserSeries
                .Where(x => x.UserId == order.UserId && x.SeriesId == order.SeriesId)
                .FirstOrDefaultAsync();

            var series = await _context.Series
                .Where(x => x.Id == order.SeriesId)
                .Include(x => x.Courses)
                .FirstOrDefaultAsync();

            if (userSeries != null)
            {
                userSeries.IsCompleted = false;
                userSeries.IsExpired = false;

                _context.UserSeries.Update(userSeries);
            }
            else
            {
                userSeries = new UserSeries
                {
                    UserId = order.UserId,
                    SeriesId = order.SeriesId!.Value,
                };
                await _context.AddAsync(userSeries);
            }

            foreach (var item in series!.Courses)
            {
                var seriesProgress = _context.SeriesProgress
                    .Where(x => x.UserSeriesId == userSeries.Id && x.CourseId == item.CourseId)
                    .FirstOrDefault();

                if (seriesProgress != null)
                {
                    seriesProgress.Progress = 0;
                    seriesProgress.Order = item.Order;
                    seriesProgress.IsCompleted = false;

                    _context.SeriesProgress.Update(seriesProgress);
                }
                else
                {
                    seriesProgress = new SeriesProgress
                    {
                        UserSeriesId = userSeries.Id,
                        CourseId = item.CourseId,
                        Order = item.Order
                    };
                    await _context.AddAsync(seriesProgress);
                }
            }
        }

        int saved = await _context.SaveChangesAsync();

        return saved > 0
            ? new SuccessResult("Payment completed successfully.")
            : new ErrorResult("Payment aknowleded successfully, however an issue occurred while saving your order details, kindly contact the support team.");
    }


    #region PRIVATE METHODS

    /// <summary>
    /// Initiates a transaction with the payment provider.
    /// </summary>
    private async Task<PaystackResponse<TransactionResponse>> InitiateTransaction(InitiateTransactionModel model)
    {
        StringContent jsonContent = model.ToJsonContent();

        using HttpResponseMessage httpResponse = await _paystackClient.PostAsync("/transaction/initialize", jsonContent);

        httpResponse.EnsureSuccessStatusCode();

        string responseString = await httpResponse.Content.ReadAsStringAsync();
        var response = JsonConvert.DeserializeObject<PaystackResponse<TransactionResponse>>(responseString);

        return response;
    }

    /// <summary>
    /// Verifies a transaction with the payment provider.
    /// </summary>
    private async Task<PaystackResponse<VerifyTransactionResponse>> VerifyTransaction(string reference)
    {
        using HttpResponseMessage httpResponse = await _paystackClient.GetAsync($"/transaction/verify/{reference}");

        string responseString = await httpResponse.Content.ReadAsStringAsync();
        var response = JsonConvert.DeserializeObject<PaystackResponse<VerifyTransactionResponse>>(responseString);

        return response;
    }

    #endregion
}
