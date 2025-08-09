namespace InChambers.Core.Models.View.Videos;

public class VideoDetailView
{
    public string VideoId { get; set; } = null!;
    public string Token { get; set; } = null!;
    public string PreviewVideoId { get; set; }
    public string ThumbnailUrl { get; set; }
}