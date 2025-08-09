using System.Text.Json.Serialization;

namespace InChambers.Core.Models.View;

public class PriceView
{
    public int DurationId { get; set; }
    public required string Name { get; set; }
    public decimal Price { get; set; }

    [JsonIgnore]
    public bool IsDeleted { get; set; }
}