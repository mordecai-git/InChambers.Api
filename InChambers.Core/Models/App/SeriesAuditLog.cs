using System.ComponentModel.DataAnnotations;

namespace InChambers.Core.Models.App;

public class SeriesAuditLog : BaseAppModel
{
    public int SeriesId { get; set; }
    [MaxLength(1000)]
    public required string Description { get; set; }
    public int CreatedById { get; set; }

    public Series Series { get; set; }
    public User CreatedBy { get; set; }
}