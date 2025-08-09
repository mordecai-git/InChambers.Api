using System.ComponentModel.DataAnnotations;

namespace InChambers.Core.Models.App;

public class BaseAppModel
{
    [Required]
    public int Id { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}