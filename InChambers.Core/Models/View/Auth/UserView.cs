using System.Text.Json.Serialization;

namespace InChambers.Core.Models.View.Auth;

public class UserView
{
    [JsonIgnore]
    public int Id { get; set; }

    public string Uid { get; set; }
    public string Type { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Phone { get; set; }
}