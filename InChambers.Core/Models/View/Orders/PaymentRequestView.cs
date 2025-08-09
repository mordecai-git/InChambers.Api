using System.Text.Json.Serialization;

namespace InChambers.Core.Models.View.Orders;

/// <summary>
/// Represents a view model for displaying payment request information.
/// </summary>
public class PaymentRequestView
{
    /// <summary>
    /// Identifier for the payment request.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// URL for user authorization in the payment request.
    /// </summary>
    public string Authorization_Url { get; set; }

    /// <summary>
    /// Access code associated with the payment request.
    /// </summary>
    public string Access_Code { get; set; }

    /// <summary>
    /// Reference identifier for the payment request.
    /// </summary>
    public string Reference { get; set; }

    /// <summary>
    /// Indicates whether the payment associated with the request has been made (JsonIgnore).
    /// </summary>
    [JsonIgnore]
    public bool IsPaid { get; set; }
}