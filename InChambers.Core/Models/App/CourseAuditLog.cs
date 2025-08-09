using System.ComponentModel.DataAnnotations;

namespace InChambers.Core.Models.App;

public class CourseAuditLog : BaseAppModel
{
    public int CourseId { get; set; }
    [MaxLength(1000)]
    public required string Description { get; set; }
    public int CreatedById { get; set; }

    public Course Course { get; set; }
    public User CreatedBy { get; set; }
}