using Newtonsoft.Json;

namespace InChambers.Core.Models.View.AnnotatedAgreement;

public class AnnotatedAgreementView
{
    [JsonIgnore]
    public int Id { get; set; }
    public string Uid { get; set; }
    public required string Title { get; set; }
    public required string Summary { get; set; }
    public int TypeId { get; set; }
    public required string TypeName { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAtUtc { get; set; }
    public string DocumentThumbnailUrl { get; set; }
}
