namespace InChambers.Core.Models.ApiVideo.Response;

public class ApiVideoDetail
{
    public ApiVideoAsset Assets { get; set; } = null!;
    public string VideoId { get; set; } = null!;
}

public class ApiVideoAsset
{
    public string Player { get; set; } = null!;
    public string Thumbnail { get; set; } = null!;
}