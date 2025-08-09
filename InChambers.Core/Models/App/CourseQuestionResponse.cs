namespace InChambers.Core.Models.App;

public class CourseQuestionResponse : BaseAppModel
{
    public int CourseId { get; set; }
    public int QuestionId { get; set; }
    public string Answer { get; set; }
    public int? OptionId { get; set; }

    public int CreatedById { get; set; }

    public Course Course { get; set; }
    public CourseQuestion Question { get; set; }
    public CourseQuestionOption Option { get; set; }
    public User CreatedBy { get; set; }
}