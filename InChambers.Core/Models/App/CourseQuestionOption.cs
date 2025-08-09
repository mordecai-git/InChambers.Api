using System.ComponentModel.DataAnnotations;
using InChambers.Core.Interfaces;

namespace InChambers.Core.Models.App;

public class CourseQuestionOption : BaseAppModel, ISoftDeletable
{
    public int QuestionId { get; set; }
    [Required][MaxLength(255)] public string Value { get; set; } = null!;

    public CourseQuestion Question { get; set; }

    public int CreatedById { get; set; }
    public int? UpdatedById { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }

    public bool IsDeleted { get; set; }
    public int? DeletedById { get; set; }
    public DateTime? DeletedOnUtc { get; set; }
}