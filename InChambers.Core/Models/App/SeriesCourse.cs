using InChambers.Core.Interfaces;

namespace InChambers.Core.Models.App;

public class SeriesCourse : BaseAppModel, ISoftDeletable
{
    public int SeriesId { get; set; }
    public int CourseId { get; set; }
    public int Order { get; set; }

    public int CreatedById { get; set; }
    public int? UpdatedById { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }

    public bool IsDeleted { get; set; } = false;
    public int? DeletedById { get; set; }
    public DateTime? DeletedOnUtc { get; set; }

    public Series Series { get; set; }
    public Course Course { get; set; }
    public User CreatedBy { get; set; }
    public User UpdatedBy { get; set; }
}