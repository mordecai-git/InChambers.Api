namespace InChambers.Core.Models.Paystack;

/// <summary>
/// Represents the model for initiating a transaction.
/// </summary>
public class InitiateTransactionModel
{
    /// <summary>
    /// Email associated with the transaction initiation.
    /// </summary>
    public required string email { get; set; }

    /// <summary>
    /// Amount for the transaction initiation.
    /// </summary>
    public required string amount { get; set; }

    /// <summary>
    /// The URL that will be called upon successful payment
    /// </summary>
    public required string callback_url { get; set; }

    /// <summary>
    /// Additional metadata information for the transaction initiation.
    /// </summary>
    public required string metadata { get; set; }
}