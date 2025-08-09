namespace InChambers.Core.Models.View.Questions;

public class CourseQuestionView
{
    public int Id { get; set; }
    public string CourseUid { get; set; } = null!;
    public string Text { get; set; } = null!;
    public bool IsMultiple { get; set; }
    public bool IsRequired { get; set; }
    public List<QuestionOptionView> Options { get; set; } = new();
}