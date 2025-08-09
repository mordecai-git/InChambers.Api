using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using InChambers.Core.Interfaces;

namespace InChambers.Core.Models.App;

public class SmeHub : BaseAppModel, ISoftDeletable
{
    [Required][MaxLength(200)] public string Uid { get; set; } = null!;

    [Required][MaxLength(100)] public string Title { get; set; } = null!;

    [Required] public int TypeId { get; set; }

    [Required][MaxLength(255)] public string Summary { get; set; } = null!;

    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    [Column(TypeName = "nvarchar(MAX)")]
    [Required]
    public string Description { get; set; } = null!;

    [Required]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Price { get; set; }

    public int DocumentId { get; set; }

    public string Tags { get; set; }

    [Required] public bool IsActive { get; set; } = true;

    public int CreatedById { get; set; }
    public int? UpdatedById { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }
    public int ViewCount { get; set; } = 0;

    [Required] public bool IsDeleted { get; set; } = false;
    public int? DeletedById { get; set; }
    public DateTime? DeletedOnUtc { get; set; }

    public SmeHubType Type { get; set; }
    public Document Document { get; set; }
    public User CreatedBy { get; set; }
    public User UpdatedBy { get; set; }
    public User DeletedBy { get; set; }
}
