using InChambers.Core.Models.View.Courses;

namespace InChambers.Core.Models.View.Series;

public class SeriesCourseView
{
    public required string SeriesUid { get; set; }
    public required string CourseUid { get; set; }
    public int Order { get; set; }

    public CourseView Course { get; set; }
}
