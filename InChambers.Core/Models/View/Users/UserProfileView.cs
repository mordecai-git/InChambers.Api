namespace InChambers.Core.Models.View.Users;

public class UserProfileView
{
    public Guid Uid { get; set; } = new Guid();
    public required string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Phone { get; set; }
    public DateTime LastLoginDate { get; set; }
    public bool EmailConfirmed { get; set; }
    public List<string> Roles { get; set; } = new List<string>();
}