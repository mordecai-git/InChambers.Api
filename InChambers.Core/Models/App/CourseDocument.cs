using Microsoft.EntityFrameworkCore;

namespace InChambers.Core.Models.App;

[Index(nameof(CourseId), nameof(DocumentId))]
public class CourseDocument : BaseAppModel
{
    public int CourseId { get; set; }
    public int DocumentId { get; set; }

    public Course Course { get; set; }
    public Document Document { get; set; }
    public int CreatedById { get; set; }
    public User CreatedBy { get; set; }
}