using System.ComponentModel.DataAnnotations;

namespace InChambers.Core.Models.App;

public class Document : BaseAppModel
{
    [StringLength(255)]
    public required string Name { get; set; }

    [StringLength(25)]
    public required string Type { get; set; }

    [MaxLength(255)]
    public required string Url { get; set; }

    [MaxLength(255)]
    public required string ThumbnailUrl { get; set; }

    [Required]
    public int CreatedById { get; set; }

    public User CreatedBy { get; set; }
}