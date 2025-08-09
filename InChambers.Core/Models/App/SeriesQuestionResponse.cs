namespace InChambers.Core.Models.App;

public class SeriesQuestionResponse : BaseAppModel
{
    //public int SeriesId { get; set; }
    //public int CourseId { get; set; }
    public int QuestionId { get; set; }
    public string Answer { get; set; }
    public int? OptionId { get; set; }

    public int CreatedById { get; set; }

    //public Series Series { get; set; }
    //public Course Course { get; set; }
    public SeriesQuestion Question { get; set; }
    public SeriesQuestionOption Option { get; set; }
    public User CreatedBy { get; set; }
}