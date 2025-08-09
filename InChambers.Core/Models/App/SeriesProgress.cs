using System.ComponentModel.DataAnnotations.Schema;

namespace InChambers.Core.Models.App;

public class SeriesProgress : BaseAppModel
{
    public int UserSeriesId { get; set; }
    public int CourseId { get; set; }
    public int Order { get; set; }
    [Column(TypeName = "decimal(20, 12)")]
    public decimal Progress { get; set; } = 0;
    public bool IsCompleted { get; set; } = false;
    public Course Course { get; internal set; }
}
