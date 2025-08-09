using Newtonsoft.Json;

namespace InChambers.Core.Models.View.Courses;

public class CourseView
{
    [JsonIgnore] public int Id { get; set; }
    public required string Uid { get; set; }
    public required string Title { get; set; }
    public required string Summary { get; set; }
    public int CreatedById { get; set; }
    public bool IsPublished { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? PublishedOnUtc { get; set; }
    public string ThumbnailUrl { get; set; }
    public List<PriceView> Prices { get; set; } = new();
    public string Duration { get; set; }
    public bool HasBought { get; set; }
}