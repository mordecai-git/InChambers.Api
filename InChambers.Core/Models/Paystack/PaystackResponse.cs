namespace InChambers.Core.Models.Paystack;

/// <summary>
/// Represents a generic Paystack response with status, message, and generic data payload.
/// </summary>
/// <typeparam name="T">Type of the data payload.</typeparam>
public class PaystackResponse<T>
{
    /// <summary>
    /// Status of the Paystack response.
    /// </summary>
    public bool status { get; set; }

    /// <summary>
    /// Message associated with the Paystack response.
    /// </summary>
    public string message { get; set; }

    /// <summary>
    /// Data payload of the Paystack response, generic type.
    /// </summary>
    public T data { get; set; }
}