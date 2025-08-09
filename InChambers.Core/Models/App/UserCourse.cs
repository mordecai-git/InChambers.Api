using System.ComponentModel.DataAnnotations.Schema;

namespace InChambers.Core.Models.App;

public class UserCourse : BaseAppModel
{
    public int UserId { get; set; }
    public int CourseId { get; set; }

    [Column(TypeName = "decimal(20, 12)")]
    public decimal Progress { get; set; } = 0;

    public bool IsCompleted { get; set; } = false;
    public bool IsExpired { get; set; } = false; // TODO: create a job that expires user's courses

    public Course Course { get; set; }
}