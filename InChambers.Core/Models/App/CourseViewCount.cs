namespace InChambers.Core.Models.App;

public class CourseViewCount : BaseAppModel
{
    public int CourseId { get; set; }
    public int? ViewedById { get; set; }
    public string IpAddress { get; set; }

    public Course Course { get; set; }
    public User ViewedBy { get; set; }
}