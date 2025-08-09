using Mapster;
using InChambers.Core.Interfaces;
using InChambers.Core.Models.App;
using InChambers.Core.Models.Input.Auth;
using InChambers.Core.Models.Input.Questions;
using InChambers.Core.Models.Utilities;
using InChambers.Core.Models.View.Questions;
using Microsoft.EntityFrameworkCore;

namespace InChambers.Core.Services;

public class QuestionService : IQuestionService
{
    private readonly InChambersContext _context;
    private readonly UserSession _userSession;

    public QuestionService(InChambersContext context, UserSession userSession)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _userSession = userSession ?? throw new ArgumentNullException(nameof(userSession));
    }

    #region Course Questions
    public async Task<Result> AddQuestionToCourse(string courseUid, QuestionAndAnswerModel model)
    {
        if (model.IsMultiple && !model.Options.Any())
            return new ErrorResult("Multiple choice question must have options");

        int courseId = await _context.Courses
            .Where(c => c.Uid == courseUid && !c.IsDeleted)
            .Select(c => c.Id)
            .FirstOrDefaultAsync();

        if (courseId == 0)
            return new ErrorResult("Course not found");

        var newQuestion = model.Adapt<CourseQuestion>();
        newQuestion.CourseId = courseId;
        newQuestion.CreatedById = _userSession.UserId;
        if (model.IsMultiple)
        {
            newQuestion.Options = model.Options.Select(o => new CourseQuestionOption
            {
                Value = o.Value,
                CreatedById = _userSession.UserId
            }).ToList();
        }

        await _context.AddAsync(newQuestion);

        int saved = await _context.SaveChangesAsync();

        return saved > 0
            ? new SuccessResult("Question added to course successfully")
            : new ErrorResult("Failed to add question to course");
    }

    public async Task<Result> UpdateCourseQuestion(string courseUid, QuestionAndAnswerModel model)
    {
        if (model.IsMultiple && !model.Options.Any())
            return new ErrorResult("Multiple choice question must have options");

        var question = await _context.CourseQuestions
            .Include(q => q.Options)
            .Where(q => q.Course!.Uid == courseUid && q.Id == model.Id && !q.IsDeleted)
            .FirstOrDefaultAsync();

        if (question == null)
            return new ErrorResult("Question not found");

        if (question.IsMultiple && !model.IsMultiple)
        {
            _context.CourseQuestionOptions.RemoveRange(question.Options);
        }
        else if (model.IsMultiple)
        {
            // add new options
            foreach (var opt in model.Options)
            {
                var option = question.Options.FirstOrDefault(o => o.Id == opt.Id);
                if (option == null)
                {
                    question.Options.Add(new CourseQuestionOption
                    {
                        Value = opt.Value,
                        CreatedById = _userSession.UserId
                    });
                }
                else
                {
                    option.Value = opt.Value;
                    option.UpdatedById = _userSession.UserId;
                    option.UpdatedOnUtc = DateTime.UtcNow;
                }
            }

            // remove deleted options
            var optionIds = model.Options.Select(o => o.Id);
            var deletedOptions = question.Options.Where(o => !optionIds.Contains(o.Id)).ToList();
            _context.CourseQuestionOptions.RemoveRange(deletedOptions);
        }

        // Update questions
        question.Text = model.Text;
        question.IsMultiple = model.IsMultiple;
        question.IsRequired = model.IsRequired;

        question.UpdatedById = _userSession.UserId;
        question.UpdatedOnUtc = DateTime.UtcNow;

        int saved = await _context.SaveChangesAsync();

        return saved > 0
            ? new SuccessResult("Question updated successfully")
            : new ErrorResult("Failed to update question");
    }

    public async Task<Result> DeleteCourseQuestion(int questionId)
    {
        var question = await _context.CourseQuestions
            .Where(q => q.Id == questionId && !q.IsDeleted)
            .FirstOrDefaultAsync();

        if (question == null)
            return new ErrorResult("Question not found");

        _context.CourseQuestions.Remove(question);

        int saved = await _context.SaveChangesAsync();

        return saved > 0
            ? new SuccessResult("Question deleted successfully")
            : new ErrorResult("Failed to delete question");
    }

    public async Task<Result> ListCourseQuestions(string courseUid)
    {
        var questions = await _context.CourseQuestions
            .Include(q => q.Options)
            .Where(q => q.Course!.Uid == courseUid && !q.IsDeleted)
            .ProjectToType<CourseQuestionView>()
            .ToListAsync();

        return new SuccessResult(questions);
    }

    public async Task<Result> AddAnswersForCourse(string courseUid, List<QuestionResponseModel> model)
    {
        var course = await _context.Courses
          .Where(c => c.Uid == courseUid && !c.IsDeleted)
          .FirstOrDefaultAsync();

        if (course is null)
            return new ErrorResult("Course not found");

        var answers = model.Select(m => new CourseQuestionResponse
        {
            QuestionId = m.QuestionId,
            Answer = m.Answer,
            OptionId = m.OptionId,
            CreatedById = _userSession.UserId
        });

        await _context.AddRangeAsync(answers);

        int saved = await _context.SaveChangesAsync();

        return saved > 0
            ? new SuccessResult("Answers added successfully")
            : new ErrorResult("Failed to add answers");
    }

    #endregion

    #region Series Questions
    public async Task<Result> AddQuestionToSeries(string seriesUid, QuestionAndAnswerModel model)
    {
        if (model.IsMultiple && !model.Options.Any())
            return new ErrorResult("Multiple choice question must have options");

        int seriesId = await _context.Series
            .Where(s => s.Uid == seriesUid && !s.IsDeleted)
            .Select(s => s.Id)
            .FirstOrDefaultAsync();

        if (seriesId == 0)
            return new ErrorResult("Series not found");

        var course = await _context.Courses
            .Where(c => c.Uid == model.CourseUid)
            .FirstOrDefaultAsync();

        if (course is null)
            return new ErrorResult("Course not found");

        var newQuestion = model.Adapt<SeriesQuestion>();
        newQuestion.SeriesId = seriesId;
        newQuestion.Course = course;
        newQuestion.CreatedById = _userSession.UserId;
        if (model.IsMultiple)
        {
            newQuestion.Options = model.Options.Select(o => new SeriesQuestionOption
            {
                Value = o.Value,
                CreatedById = _userSession.UserId
            }).ToList();
        }

        await _context.AddAsync(newQuestion);

        int saved = await _context.SaveChangesAsync();

        return saved > 0
            ? new SuccessResult("Question added to series successfully", newQuestion.Adapt<SeriesQuestionView>())
            : new ErrorResult("Failed to add question to series");
    }

    public async Task<Result> UpdateSeriesQuestion(string seriesUid, QuestionAndAnswerModel model)
    {
        if (model.IsMultiple && !model.Options.Any())
            return new ErrorResult("Multiple choice question must have options");

        var question = await _context.SeriesQuestions
            .Include(q => q.Options)
            .Where(q => q.Series!.Uid == seriesUid && q.Course!.Uid == model.CourseUid && q.Id == model.Id && !q.IsDeleted)
            .FirstOrDefaultAsync();

        if (question == null)
            return new ErrorResult("Question not found");

        if (question.IsMultiple && !model.IsMultiple)
        {
            _context.SeriesQuestionOptions.RemoveRange(question.Options);
        }
        else if (model.IsMultiple)
        {
            // add new options
            foreach (var opt in model.Options)
            {
                var option = question.Options.FirstOrDefault(o => o.Id == opt.Id);
                if (option == null)
                {
                    question.Options.Add(new SeriesQuestionOption
                    {
                        Value = opt.Value,
                        CreatedById = _userSession.UserId
                    });
                }
                else
                {
                    option.Value = opt.Value;
                    option.UpdatedById = _userSession.UserId;
                    option.UpdatedOnUtc = DateTime.UtcNow;
                }
            }

            // remove deleted options
            var optionIds = model.Options.Select(o => o.Id);
            var deletedOptions = question.Options.Where(o => !optionIds.Contains(o.Id)).ToList();
            _context.SeriesQuestionOptions.RemoveRange(deletedOptions);
        }

        // Update questions
        question.Text = model.Text;
        question.IsMultiple = model.IsMultiple;
        question.IsRequired = model.IsRequired;

        question.UpdatedById = _userSession.UserId;
        question.UpdatedOnUtc = DateTime.UtcNow;

        int saved = await _context.SaveChangesAsync();

        return saved > 0
            ? new SuccessResult("Question updated successfully")
            : new ErrorResult("Failed to update question");
    }

    public async Task<Result> DeleteSeriesQuestion(int questionId)
    {
        var question = await _context.SeriesQuestions
            .Where(q => q.Id == questionId && !q.IsDeleted)
            .FirstOrDefaultAsync();

        if (question == null)
            return new ErrorResult("Question not found");

        _context.SeriesQuestions.Remove(question);

        int saved = await _context.SaveChangesAsync();

        return saved > 0
            ? new SuccessResult("Question deleted successfully")
            : new ErrorResult("Failed to delete question");
    }

    public async Task<Result> ListSeriesQuestions(string seriesUid, string courseUid)
    {
        var questions = await _context.SeriesQuestions
            .Include(q => q.Options)
            .Where(q => q.Series!.Uid == seriesUid && q.Course!.Uid == courseUid && !q.IsDeleted)
            .ProjectToType<SeriesQuestionView>()
            .ToListAsync();

        return new SuccessResult(questions);
    }

    public async Task<Result> AddAnswersForSeries(string seriesUid, string courseUid, List<QuestionResponseModel> model)
    {
        int seriesId = await _context.Series
          .Where(s => s.Uid == seriesUid && !s.IsDeleted)
          .Select(s => s.Id)
          .FirstOrDefaultAsync();

        if (seriesId == 0)
            return new ErrorResult("Series not found");

        var courseId = await _context.Courses
          .Where(c => c.Uid == courseUid)
          .Select(c => c.Id)
          .FirstOrDefaultAsync();

        if (courseId == 0)
            return new ErrorResult("Course not found");

        var answers = model.Select(m => new SeriesQuestionResponse
        {
            //SeriesId = seriesId,
            //CourseId = courseId,
            QuestionId = m.QuestionId,
            Answer = m.Answer,
            OptionId = m.OptionId,
            CreatedById = _userSession.UserId
        });

        await _context.AddRangeAsync(answers);

        int saved = await _context.SaveChangesAsync();

        return saved > 0
            ? new SuccessResult("Answers added successfully")
            : new ErrorResult("Failed to add answers");
    }

    #endregion
}