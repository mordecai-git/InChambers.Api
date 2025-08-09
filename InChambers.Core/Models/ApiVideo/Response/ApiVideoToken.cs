using System.Text.Json.Serialization;

namespace InChambers.Core.Models.ApiVideo.Response;

public class ApiVideoToken
{
    public string Token { get; set; }
    [JsonIgnore]
    public int Ttl { get; set; }
    [JsonIgnore]
    public DateTime CreatedAt { get; set; }
    [JsonIgnore]
    public DateTime? ExpiresAt { get; set; }
}
