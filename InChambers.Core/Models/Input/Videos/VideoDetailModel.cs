using FluentValidation;

namespace InChambers.Core.Models.Input.Videos;

public class VideoDetailModel
{
    public string videoId { get; set; } = null!;
    public string title { get; set; } = null!;
    public string playerId { get; set; }
    public bool mp4Support { get; set; } = false;
    public bool @public { get; set; } = false;
    public int Duration { get; set; }
    public string ThumbnailUrl { get; set; }
}

public class VideoDetailValidator : AbstractValidator<VideoDetailModel>
{
    public VideoDetailValidator()
    {
        RuleFor(x => x.videoId).NotEmpty();
        RuleFor(x => x.title).NotEmpty();
    }
}