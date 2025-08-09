using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InChambers.Core.Models.App;

public class Discount : BaseAppModel
{
    [Required]
    [MaxLength(255)]
    public required string Code { get; set; }
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Amount { get; set; }
    public bool IsActive { get; set; }
    public bool IsPercentage { get; set; }
    public bool IsSingleUse { get; set; }
    public int TotalAvailable { get; set; } = -1;
    public int TotalUsed { get; set; } = 0;
    [Column(TypeName = "decimal(18, 2)")]
    public decimal? MinAmount { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsDeleted { get; set; }

    public int CreatedById { get; set; }
    public int? UpdatedById { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }
    public int? DeletedById { get; set; }
    public DateTime? DeletedOnUtc { get; set; }
}
