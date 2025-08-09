namespace InChambers.Core.Models.View.Series;

public class SeriesDetailView : SeriesView
{
    public required string Description { get; set; }
    public List<string> Tags { get; set; } = new();
    public int? UpdatedById { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }
    public bool IsDeleted { get; set; }
    public ReferencedUserView CreatedBy { get; set; }
    public ReferencedUserView UpdatedBy { get; set; }
    public List<PriceView> Prices { get; set; } = new();
}