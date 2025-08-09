using System.ComponentModel.DataAnnotations;
using InChambers.Core.Interfaces;

namespace InChambers.Core.Models.App;

public class SeriesQuestion : BaseAppModel, ISoftDeletable
{
    public int SeriesId { get; set; }
    public int CourseId { get; set; }
    [Required][MaxLength(500)] public string Text { get; set; } = null!;
    public bool IsMultiple { get; set; }
    public bool IsRequired { get; set; }

    public int CreatedById { get; set; }

    public int? UpdatedById { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }

    public bool IsDeleted { get; set; }
    public int? DeletedById { get; set; }
    public DateTime? DeletedOnUtc { get; set; }

    public Series Series { get; set; }
    public Course Course { get; set; }
    public User CreatedBy { get; set; }
    public User UpdatedBy { get; set; }
    public ICollection<SeriesQuestionOption> Options { get; set; } = new List<SeriesQuestionOption>();
}