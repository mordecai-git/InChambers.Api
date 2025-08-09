using System.ComponentModel.DataAnnotations;

namespace InChambers.Core.Models.App;

public class InvitedUser : BaseAppModel
{
    [Required]
    [MaxLength(50)]
    public required string Email { get; set; }

    [Required]
    [MaxLength(50)]
    public required string FirstName { get; set; }

    [Required]
    [MaxLength(50)]
    public required string LastName { get; set; }

    [Required]
    [MaxLength(25)]
    public required string Phone { get; set; }
    public bool CanManageCourses { get; set; }
    public bool CanManageUsers { get; set; }

    public int EmailsSent { get; set; } = 1;
    public bool IsAccepted { get; set; }
    public DateTime? DateAccepted { get; set; }
    public int CreatedById { get; set; }

    [Required]
    [StringLength(255)]
    public string Token { get; set; } = null!;

    [Required]
    public DateTime TokenExpiry { get; set; } = DateTime.UtcNow.AddDays(7);

    public User CreatedBy { get; set; }
}