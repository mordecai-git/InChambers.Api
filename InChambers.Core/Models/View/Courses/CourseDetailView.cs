using InChambers.Core.Models.Utilities;

namespace InChambers.Core.Models.View.Courses;

public class CourseDetailView : CourseView
{
    public required string Description { get; set; }
    public List<string> Tags { get; set; } = new();
    public int? UpdatedById { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }
    public bool IsDeleted { get; set; }
    public List<CourseLinkView> UsefulLinks { get; set; } = new();
    public IEnumerable<DocumentView> Resources { get; set; } = new List<DocumentView>();
    public ReferencedUserView CreatedBy { get; set; }
    public ReferencedUserView UpdatedBy { get; set; }

    public bool VideoIsUploaded { get; set; }
    public string PreviewVideoId { get; set; }
}