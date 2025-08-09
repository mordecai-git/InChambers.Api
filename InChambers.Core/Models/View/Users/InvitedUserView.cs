namespace InChambers.Core.Models.View.Users;

public class InvitedUserView
{
    public int Id { get; set; }
    public required string Email { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Phone { get; set; }
    public bool CanManageCourses { get; set; }
    public bool CanManageUsers { get; set; }
    public bool IsAccepted { get; set; }
}