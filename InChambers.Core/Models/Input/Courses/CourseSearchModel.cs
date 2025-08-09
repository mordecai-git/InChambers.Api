namespace InChambers.Core.Models.Input.Courses;

public class CourseSearchModel : PagingOptionModel
{
    public string SearchQuery { get; set; }
    public bool? IsActive { get; set; }
}