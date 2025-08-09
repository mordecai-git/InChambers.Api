using System.ComponentModel.DataAnnotations;

namespace InChambers.Core.Models.App;

public class Code : BaseAppModel
{
    [Required]
    [MaxLength(500)]
    public string Token { get; set; } = null!;

    [Required]
    public int OwnerId { get; set; }

    [Required]
    [MaxLength(25)]
    public string Purpose { get; set; } = null!;

    [Required]
    public DateTime ExpiryDate { get; set; }

    [Required]
    public bool Used { get; set; } = false;

    public User Owner { get; set; } = null!;
}