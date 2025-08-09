using InChambers.Core.Interfaces;
using InChambers.Core.Models.Input;
using InChambers.Core.Models.Input.Courses;
using InChambers.Core.Models.Input.Videos;
using InChambers.Core.Models.Utilities;
using InChambers.Core.Models.View.Courses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InChambers.Api.Controllers;

[ApiController]
[Route("api/v1/courses")]
[Authorize]
public class CoursesController : BaseController
{
    private readonly ICourseService _courseService;

    public CoursesController(ICourseService courseService)
    {
        _courseService = courseService ?? throw new ArgumentNullException(nameof(courseService));
    }

    /// <summary>
    /// Create a new course
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(SuccessResult<CourseView>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
    public async Task<IActionResult> CreateCourse(CourseModel model)
    {
        var res = await _courseService.CreateCourse(model);
        return ProcessResponse(res);
    }

    /// <summary>
    /// Update the details of the video after successful upload to Api.Video
    /// </summary>
    /// <param name="courseUid"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost("{courseUid}/video-details")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResult))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
    public async Task<IActionResult> SetVideoDetails(string courseUid, VideoDetailModel model)
    {
        var res = await _courseService.SetCourseVideoDetails(courseUid, model);
        return ProcessResponse(res);
    }

    /// <summary>
    /// Add a resource (file) to a course
    /// </summary>
    /// <param name="courseUid"></param>
    /// <param name="file"></param>
    /// <returns></returns>
    [HttpPost("{courseUid}/resources")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(SuccessResult<DocumentView>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
    public async Task<IActionResult> AddResourceToCourse(string courseUid, [FromForm] FileUploadModel file)
    {
        var res = await _courseService.AddResourceToCourse(courseUid, file);
        return ProcessResponse(res);
    }

    /// <summary>
    /// Retrieve a list of the files attached to a course
    /// </summary>
    /// <param name="courseUid"></param>
    /// <returns></returns>
    [HttpGet("{courseUid}/resources")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResult<List<DocumentView>>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
    public async Task<IActionResult> ListResources(string courseUid)
    {
        var res = await _courseService.ListResources(courseUid);
        return ProcessResponse(res);
    }

    /// <summary>
    /// Remove a resource (file) from a course
    /// </summary>
    /// <param name="courseUid"></param>
    /// <param name="documentId"></param>
    /// <returns></returns>
    [HttpGet("{courseUid}/resources/{documentId}/delete")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResult))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
    public async Task<IActionResult> RemoveResourceFromCourse(string courseUid, int documentId)
    {
        var res = await _courseService.RemoveResourceFromCourse(courseUid, documentId);
        return ProcessResponse(res);
    }

    /// <summary>
    /// Update a course
    /// </summary>
    /// <param name="courseUid"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost("{courseUid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResult<CourseDetailView>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
    public async Task<IActionResult> UpdateCourse(string courseUid, CourseModel model)
    {
        var res = await _courseService.UpdateCourse(courseUid, model);
        return ProcessResponse(res);
    }

    /// <summary>
    /// Delete a course
    /// </summary>
    /// <param name="courseUid"></param>
    /// <returns></returns>
    [HttpGet("{courseUid}/delete")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResult))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
    public async Task<IActionResult> DeleteCourse(string courseUid)
    {
        var res = await _courseService.DeleteCourse(courseUid);
        return ProcessResponse(res);
    }

    /// <summary>
    /// Get the details of a course
    /// </summary>
    /// <param name="courseUid"></param>
    /// <returns></returns>
    [HttpGet("{courseUid}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResult<CourseDetailView>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
    public async Task<IActionResult> GetCourse(string courseUid)
    {
        var res = await _courseService.GetCourse(courseUid);
        return ProcessResponse(res);
    }

    /// <summary>
    /// Update the view count of a course, used for analytics. This is called when a user views a course.
    /// </summary>
    /// <param name="courseUid"></param>
    /// <returns></returns>
    [HttpPost("{courseUid}/view")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResult))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
    public async Task<IActionResult> AddCourseView(string courseUid)
    {
        var res = await _courseService.AddCourseView(courseUid);
        return ProcessResponse(res);
    }

    /// <summary>
    /// List courses
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResult<List<CourseView>>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
    public async Task<IActionResult> ListCourses([FromQuery] CourseSearchModel request)
    {
        var res = await _courseService.ListCourses(request);
        return ProcessResponse(res);
    }

    /// <summary>
    /// Publish a course that is still in draft
    /// </summary>
    /// <param name="courseUid"></param>
    /// <returns></returns>
    [HttpPost("{courseUid}/publish")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResult))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
    public async Task<IActionResult> PublishCourse(string courseUid)
    {
        var res = await _courseService.PublishCourse(courseUid);
        return ProcessResponse(res);
    }

    /// <summary>
    /// Activate a course, making it available for students
    /// </summary>
    /// <param name="courseUid"></param>
    /// <returns></returns>
    [HttpPost("{courseUid}/activate")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResult))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
    public async Task<IActionResult> ActivateCourse(string courseUid)
    {
        var res = await _courseService.ActivateCourse(courseUid);
        return ProcessResponse(res);
    }

    /// <summary>
    /// Deactivate a course, making it unavailable to students
    /// </summary>
    /// <param name="courseUid"></param>
    /// <returns></returns>
    [HttpPost("{courseUid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResult))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
    public async Task<IActionResult> DeactivateCourse(string courseUid)
    {
        var res = await _courseService.DeactivateCourse(courseUid);
        return ProcessResponse(res);
    }
}