namespace InChambers.Core.Models.View.Series;

public class SeriesProgressView
{
    public required string CourseUid { get; set; }
    public int Order { get; set; }
    public decimal Progress { get; set; }
    public bool IsCompleted { get; set; }
}
