namespace InChambers.Core.Models.Paystack;

/// <summary>
/// Represents the response for verifying a transaction, including an identifier and status.
/// </summary>
public class VerifyTransactionResponse
{
    /// <summary>
    /// Identifier associated with the verified transaction.
    /// </summary>
    public long id { get; set; }

    /// <summary>
    /// Status of the verified transaction.
    /// </summary>
    public string status { get; set; }
}