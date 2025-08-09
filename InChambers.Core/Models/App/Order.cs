using System.ComponentModel.DataAnnotations.Schema;

namespace InChambers.Core.Models.App;

public class Order : BaseAppModel
{
    public int UserId { get; set; }

    public required string ItemType { get; set; }
    public int? CourseId { get; set; }
    public int? SeriesId { get; set; }
    public int? SmeHubId { get; set; }
    public int? AnnotatedAgreementId { get; set; }

    public required string BillingAddress { get; set; }
    public int? DurationId { get; set; }
    public int? DiscountId { get; set; }
    [Column(TypeName = "decimal(18, 2)")]
    public decimal DiscountApplied { get; set; } = 0m;
    [Column(TypeName = "decimal(18, 2)")]
    public decimal ItemAmount { get; set; }
    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalAmount { get; set; }

    public int? UpdateById { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public bool IsPaid { get; set; } = false;
    public DateTime? PaidAt { get; set; }
    public string Authorization_Url { get; set; }
    public string Access_Code { get; set; }
    public string Reference { get; set; }

    public Duration Duration { get; set; }
    public Discount Discount { get; set; }
    public UserContent UserContent { get; set; }

    public Course Course { get; set; }
    public Series Series { get; set; }
    public SmeHub SmeHub { get; set; }
    public AnnotatedAgreement AnnotatedAgreement { get; set; }
}
