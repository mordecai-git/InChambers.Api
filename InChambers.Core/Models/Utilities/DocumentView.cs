namespace InChambers.Core.Models.Utilities;

public class DocumentView
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public required string Type { get; set; }

    public required string Url { get; set; }

    public required string ThumbnailUrl { get; set; }
}