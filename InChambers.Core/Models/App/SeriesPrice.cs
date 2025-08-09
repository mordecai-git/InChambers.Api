using System.ComponentModel.DataAnnotations.Schema;
using InChambers.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InChambers.Core.Models.App;

[Index(nameof(SeriesId), nameof(DurationId), IsUnique = true)]
public class SeriesPrice : BaseAppModel, ISoftDeletable
{
    public int SeriesId { get; set; }
    public int DurationId { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Price { get; set; }
    public bool IsDeleted { get; set; } = false;
    public int? DeletedById { get; set; }
    public DateTime? DeletedOnUtc { get; set; }

    public virtual Series Series { get; set; }
    public virtual Duration Duration { get; set; }
}