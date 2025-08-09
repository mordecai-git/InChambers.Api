using InChambers.Core.Models.App.Constants;

namespace InChambers.Core.Models.Input.Auth;

public class UserSession
{
    public bool IsAuthenticated { get; set; }
    public int UserId { get; set; }
    public string Uid { get; set; } = null!;
    public string Name { get; set; } = null!;

    private List<string> _roles = new();

    public List<string> Roles
    {
        set => _roles = value;
    }

    public bool InRole(params string[] roles)
    {
        return roles.Any(role => _roles.Contains(role));
    }

    public bool IsAnyAdmin => InRole(RolesConstants.SuperAdmin, RolesConstants.Admin);
    public bool IsAnyManager => InRole(RolesConstants.Manager, RolesConstants.ManageCourse, RolesConstants.ManageUser);
    public bool IsOnlyManager => InRole(RolesConstants.Manager);
    public bool IsCourseManager => InRole(RolesConstants.Manager, RolesConstants.ManageCourse);
    public bool IsUserManager => InRole(RolesConstants.Manager, RolesConstants.ManageUser);
    public bool IsCustomer => InRole(RolesConstants.Customer);
    public bool IsSuperAdmin => InRole(RolesConstants.SuperAdmin);
    public bool IsAdmin => InRole(RolesConstants.Admin);
}