namespace InChambers.Core.Models.App;

public class UserSeries : BaseAppModel
{
    public int UserId { get; set; }
    public int SeriesId { get; set; }
    public bool IsCompleted { get; set; } = false;
    public bool IsExpired { get; set; } = false; // TODO: create a job that expires user's series

    public Series Series { get; set; }
}
