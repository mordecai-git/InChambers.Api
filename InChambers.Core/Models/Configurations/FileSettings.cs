namespace InChambers.Core.Models.Configurations;

public class FileSettings
{
    public required string BaseFolder { get; set; }
    public required string RequestPath { get; set; }
    public int MaxSizeLength { get; set; }
    public List<string> PermittedFileTypes { get; set; } = new();
}