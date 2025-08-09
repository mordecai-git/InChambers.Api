namespace InChambers.Core.Models.App.Constants;

public enum Roles
{
    SuperAdmin = 1,
    Admin = 2,
    Manager = 3,
    ManageCourse = 4,
    ManageUser = 5,
    Customer = 6
}

public static class RolesConstants
{
    public static readonly string SuperAdmin = "SuperAdmin";
    public static readonly string Admin = "Admin";
    public static readonly string Manager = "Manager";
    public static readonly string ManageCourse = "ManageCourse";
    public static readonly string ManageUser = "ManageUser";
    public static readonly string Customer = "Customer";
}