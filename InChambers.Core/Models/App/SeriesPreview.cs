using System.ComponentModel.DataAnnotations;

namespace InChambers.Core.Models.App;

public class SeriesPreview : BaseAppModel
{
    public int SeriesId { get; set; }
    [Required][MaxLength(50)] public string UploadToken { get; set; } = null!;
    [MaxLength(255)] public string VideoId { get; set; }
    [MaxLength(255)] public string ThumbnailUrl { get; set; }
    public int VideoDuration { get; set; }
    public bool IsUploaded { get; set; } = false;
    public Series Series { get; set; }
}