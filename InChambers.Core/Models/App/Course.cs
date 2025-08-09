using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using InChambers.Core.Interfaces;

namespace InChambers.Core.Models.App;

public class Course : BaseAppModel, ISoftDeletable
{
    [Required][MaxLength(200)] public string Uid { get; set; } = null!;

    [Required][MaxLength(100)] public string Title { get; set; } = null!;

    [Required][MaxLength(255)] public string Summary { get; set; } = "null!";

    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    [Column(TypeName = "nvarchar(MAX)")]
    public string Description { get; set; }

    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string Tags { get; set; }

    [Required] public bool IsActive { get; set; } = false;

    public int CreatedById { get; set; }
    public int? UpdatedById { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }
    public bool IsPublished { get; set; } = false;
    public int? PublishedById { get; set; }
    public DateTime? PublishedOnUtc { get; set; }
    public int ViewCount { get; set; } = 0;

    [Required] public bool IsDeleted { get; set; } = false;
    public int? DeletedById { get; set; }
    public DateTime? DeletedOnUtc { get; set; }

    public bool ForSeriesOnly { get; set; } = false;

    public User CreatedBy { get; set; }
    public User UpdatedBy { get; set; }
    public User DeletedBy { get; set; }
    public CourseVideo Video { get; set; }
    public List<CourseLink> UsefulLinks { get; set; } = new();
    public List<CourseAuditLog> AuditLogs { get; set; } = new();
    public List<CoursePrice> Prices { get; set; } = new();
    public IQueryable<CourseQuestion> QuestionAndAnswers { get; set; } = new List<CourseQuestion>().AsQueryable();
    public List<SeriesCourse> SeriesCourses { get; set; } = new();
    public List<CourseDocument> Resources { get; set; } = new();
    public List<Order> Orders { get; set; } = new();
    public List<UserCourse> UserCourses { get; set; } = new();
}