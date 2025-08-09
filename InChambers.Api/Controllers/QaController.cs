using InChambers.Core.Interfaces;
using InChambers.Core.Models.Input.Questions;
using InChambers.Core.Models.Utilities;
using InChambers.Core.Models.View.Questions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InChambers.Api.Controllers;

[ApiController]
[Route("api/v1/questions")]
[Authorize]
public class QuestionController : BaseController
{
    private readonly IQuestionService _questionService;

    public QuestionController(IQuestionService questionService)
        => _questionService = questionService ?? throw new ArgumentNullException(nameof(questionService));

    /// <summary>
    /// Add a question to a course
    /// </summary>
    /// <param name="courseUid"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost("courses/{courseUid}")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(SuccessResult))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
    public async Task<IActionResult> AddQuestionToCourse(string courseUid, QuestionAndAnswerModel model)
    {
        var res = await _questionService.AddQuestionToCourse(courseUid, model);
        return ProcessResponse(res);
    }

    /// <summary>
    /// Update the question of a course
    /// </summary>
    /// <param name="courseUid"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost("courses/{courseUid}/update")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(SuccessResult))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
    public async Task<IActionResult> UpdateCourseQuestion(string courseUid, QuestionAndAnswerModel model)
    {
        var res = await _questionService.UpdateCourseQuestion(courseUid, model);
        return ProcessResponse(res);
    }

    /// <summary>
    /// Delete a question for a course
    /// </summary>
    /// <param name="questionId"></param>
    /// <returns></returns>
    [HttpGet("courses/{questionId}/delete")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(SuccessResult))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
    public async Task<IActionResult> DeleteCourseQuestion(int questionId)
    {
        var res = await _questionService.DeleteCourseQuestion(questionId);
        return ProcessResponse(res);
    }


    /// <summary>
    /// List questions for a course
    /// </summary>
    /// <param name="courseUid"></param>
    /// <returns></returns>
    [HttpGet("courses/{courseUid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResult<List<CourseQuestionView>>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
    public async Task<IActionResult> ListCourseQuestions(string courseUid)
    {
        var res = await _questionService.ListCourseQuestions(courseUid);
        return ProcessResponse(res);
    }

    /// <summary>
    /// Submit answers to a question
    /// </summary>
    /// <param name="courseUid"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost("courses/{courseUid}/answers")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(SuccessResult))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
    public async Task<IActionResult> AddAnswers(string courseUid, List<QuestionResponseModel> model)
    {
        var res = await _questionService.AddAnswersForCourse(courseUid, model);
        return ProcessResponse(res);
    }

    /// <summary>
    /// Add a question to a series
    /// </summary>
    /// <param name="seriesUid"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost("series/{seriesUid}")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(SuccessResult))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
    public async Task<IActionResult> AddQuestionToSeries(string seriesUid, QuestionAndAnswerModel model)
    {
        var res = await _questionService.AddQuestionToSeries(seriesUid, model);
        return ProcessResponse(res);
    }

    /// <summary>
    /// Update the question of a series
    /// </summary>
    /// <param name="seriesUid"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost("series/{seriesUid}/update")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(SuccessResult))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
    public async Task<IActionResult> UpdateSeriesQuestion(string seriesUid, QuestionAndAnswerModel model)
    {
        var res = await _questionService.UpdateSeriesQuestion(seriesUid, model);
        return ProcessResponse(res);
    }



    /// <summary>
    /// Delete a question in series
    /// </summary>
    /// <param name="questionId"></param>
    /// <returns></returns>
    [HttpGet("series/{questionId}/delete")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(SuccessResult))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
    public async Task<IActionResult> DeleteSeriesQuestion(int questionId)
    {
        var res = await _questionService.DeleteSeriesQuestion(questionId);
        return ProcessResponse(res);
    }

    /// <summary>
    /// List questions for a series
    /// </summary>
    /// <param name="seriesUid"></param>
    /// <param name="courseUid"></param>
    /// <returns></returns>
    [HttpGet("series/{seriesUid}/{courseUid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResult<List<SeriesQuestionView>>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
    public async Task<IActionResult> ListSeriesQuestions(string seriesUid, string courseUid)
    {
        var res = await _questionService.ListSeriesQuestions(seriesUid, courseUid);
        return ProcessResponse(res);
    }

    /// <summary>
    /// Submit answers to a question for series
    /// </summary>
    /// <param name="seriesUid"></param>
    /// <param name="courseUid"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost("series/{seriesUid}/{courseUid}/answers")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(SuccessResult))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
    public async Task<IActionResult> AddSeriesAnswers(string seriesUid, string courseUid, List<QuestionResponseModel> model)
    {
        var res = await _questionService.AddAnswersForSeries(seriesUid, courseUid, model);
        return ProcessResponse(res);
    }
}