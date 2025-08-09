namespace InChambers.Core.Models.Paystack;

/// <summary>
/// Represents the response for a transaction, including authorization URL, access code, and reference.
/// </summary>
public class TransactionResponse
{
    /// <summary>
    /// URL for user authorization in the transaction response.
    /// </summary>
    public string authorization_url { get; set; }

    /// <summary>
    /// Access code associated with the transaction response.
    /// </summary>
    public string access_code { get; set; }

    /// <summary>
    /// Reference identifier for the transaction response.
    /// </summary>
    public string reference { get; set; }
}