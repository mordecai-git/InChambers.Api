namespace InChambers.Core.Models.View;

public class VideoView
{
    public string VideoId { get; set; }
    public string ThumbnailUrl { get; set; }
    public int VideoDuration { get; set; }
    public bool IsUploaded { get; set; }
    public string PreviewVideoId { get; set; }
    public string Token { get; set; }

    public int PreviewStart { get; set; }
    public int PreviewEnd { get; set; }

    public decimal Progress { get; set; }

    public string Duration
    {
        get
        {
            var ts = TimeSpan.FromSeconds(VideoDuration);
            return ts.ToString("hh\\:mm\\:ss");
        }
    }
}