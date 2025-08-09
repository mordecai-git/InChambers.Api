namespace InChambers.Core.Models.Configurations;

/// <summary>
/// Represents the configuration settings for interacting with the Paystack API.
/// </summary>
public class PasystackConfig
{
    /// <summary>
    /// Gets or sets the name of the HTTP client used for Paystack API requests.
    /// </summary>
    public string HttpClientName { get; set; }
}