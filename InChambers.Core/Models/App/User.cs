﻿using System.ComponentModel.DataAnnotations;

namespace InChambers.Core.Models.App;

public class User : BaseAppModel
{
    public Guid Uid { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(50)]
    public required string Email { get; set; }

    [MaxLength(50)]
    public string FirstName { get; set; }

    [MaxLength(50)]
    public string LastName { get; set; }

    [MaxLength(25)]
    public string Phone { get; set; }

    [Required]
    [MaxLength(255)]
    public required string HashedPassword { get; set; }

    [Required]
    public bool IsActive { get; set; } = true;
    public bool EmailConfirmed { get; set; } = false;

    public Login Login { get; set; }
    public ICollection<UserRole> UserRoles { get; set; }
}