using System.ComponentModel.DataAnnotations;

namespace InChambers.Core.Models.App;

public class CourseVideo : BaseAppModel
{
    public int CourseId { get; set; }

    [MaxLength(50)] public string UploadToken { get; set; } = null!;
    [MaxLength(255)] public string VideoId { get; set; }
    public int VideoDuration { get; set; }

    [MaxLength(255)] public string PreviewVideoId { get; set; }
    public int PreviewStart { get; set; }
    public int PreviewEnd { get; set; }
    [MaxLength(255)] public string ThumbnailUrl { get; set; }
    public bool IsUploaded { get; set; } = false;

    public Course Course { get; set; }
}