using InChambers.Core.Models.Input.Questions;
using InChambers.Core.Models.Utilities;

namespace InChambers.Core.Interfaces;

public interface IQuestionService
{
    Task<Result> AddQuestionToCourse(string courseUid, QuestionAndAnswerModel model);
    Task<Result> UpdateCourseQuestion(string courseUid, QuestionAndAnswerModel model);
    Task<Result> DeleteCourseQuestion(int questionId);
    Task<Result> ListCourseQuestions(string courseUid);
    Task<Result> AddAnswersForCourse(string courseUid, List<QuestionResponseModel> model);

    Task<Result> AddQuestionToSeries(string seriesUid, QuestionAndAnswerModel model);
    Task<Result> UpdateSeriesQuestion(string seriesUid, QuestionAndAnswerModel model);
    Task<Result> DeleteSeriesQuestion(int questionId);
    Task<Result> ListSeriesQuestions(string seriesUid, string courseUid);
    Task<Result> AddAnswersForSeries(string seriesUid, string courseUid, List<QuestionResponseModel> model);
}