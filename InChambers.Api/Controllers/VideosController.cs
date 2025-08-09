using InChambers.Core.Interfaces;
using InChambers.Core.Models.ApiVideo.Response;
using InChambers.Core.Models.Input.Courses;
using InChambers.Core.Models.Input.Videos;
using InChambers.Core.Models.Utilities;
using InChambers.Core.Models.View;
using InChambers.Core.Models.View.Series;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InChambers.Api.Controllers;

[ApiController]
[Route("api/v1/videos")]
[Authorize]
public class VideosController : BaseController
{
    private readonly IVideoService _videoService;

    public VideosController(IVideoService videoService) => _videoService = videoService ?? throw new ArgumentNullException(nameof(videoService));

    /// <summary>
    /// Get video upload token
    /// </summary>
    /// <returns></returns>
    [HttpGet("upload-token")]
    public async Task<IActionResult> GetUploadToken([FromQuery] int expiresInSec = 0)
    {
        var res = await _videoService.GetUploadToken(expiresInSec);
        if (res.Success)
        {
            return ProcessResponse(new SuccessResult(res.Content));
        }

        return ProcessResponse(new ErrorResult(res.Message));
    }

    /// <summary>
    /// Get video upload data
    /// </summary>
    /// <param name="courseUid"></param>
    /// <returns></returns>
    [HttpGet("course/{courseUid}/upload-data")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResult<ApiVideoToken>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
    public async Task<IActionResult> GetVideoUploadData(string courseUid)
    {
        var res = await _videoService.GetVideoUploadData(courseUid);
        return ProcessResponse(res);
    }

    /// <summary>
    /// Set the preview video for a course
    /// </summary>
    /// <param name="courseUid"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost("course/{courseUid}/preview")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResult))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
    public async Task<IActionResult> SetVideoPreview(string courseUid, ApiVideoClipModel model)
    {
        var res = await _videoService.SetVideoPreview(courseUid, model);
        return ProcessResponse(res);
    }

    /// <summary>
    /// Get the video player details
    /// </summary>
    /// <param name="courseUid"></param>
    /// <returns></returns>
    [HttpGet("course/{courseUid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResult<VideoView>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
    public async Task<IActionResult> GetVideoPlayerDetails(string courseUid)
    {
        var res = await _videoService.GetVideoPlayerDetails(courseUid);
        return ProcessResponse(res);
    }

    /// <summary>
    /// Get the course progress and player details for the logged in user.
    /// </summary>
    /// <param name="courseUid"></param>
    /// <returns></returns>
    [HttpGet("course/{courseUid}/progress")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResult<VideoView>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
    public async Task<IActionResult> GetUserCourseProgress(string courseUid)
    {
        var res = await _videoService.GetUserCourseProgress(courseUid);
        return ProcessResponse(res);
    }


    /// <summary>
    /// Report course progress for the logged in user.
    /// </summary>
    /// <param name="courseUid"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost("course/{courseUid}/progress-report")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResult))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
    public async Task<IActionResult> ReportCourseProgress(string courseUid, ProgressReportModel model)
    {
        var res = await _videoService.ReportCourseProgress(courseUid, model);
        return ProcessResponse(res);
    }

    [HttpPost("course/{courseUid}/complete")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResult))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
    public async Task<IActionResult> CourseVideoCompleted(string courseUid)
    {
        var res = await _videoService.CourseVideoCompleted(courseUid);
        return ProcessResponse(res);
    }

    [HttpGet("series/{seriesUid}/progress")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResult<SeriesProgressView>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
    public async Task<IActionResult> GetUserSeriesProgress(string seriesUid)
    {
        var res = await _videoService.GetUserSeriesProgress(seriesUid);
        return ProcessResponse(res);
    }

    [HttpGet("series/{seriesUid}/course/{courseUid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResult<VideoView>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
    public async Task<IActionResult> GetSeriesCourseVideoDetail(string seriesUid, string courseUid)
    {
        var res = await _videoService.GetSeriesCourseVideoDetail(seriesUid, courseUid);
        return ProcessResponse(res);
    }

    [HttpPost("series/{seriesUid}/course/{courseUid}/progress-report")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResult))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
    public async Task<IActionResult> ReportSeriesProgress(string seriesUid, string courseUid, [FromBody] ProgressReportModel model)
    {
        var res = await _videoService.ReportSeriesCourseProgress(seriesUid, courseUid, model);
        return ProcessResponse(res);
    }

    [HttpPost("series/{seriesUid}/course/{courseUid}/complte")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResult))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
    public async Task<IActionResult> SeriesCourseVideoCompleted(string seriesUid, string courseUid)
    {
        var res = await _videoService.SeriesCourseVideoCompleted(seriesUid, courseUid);
        return ProcessResponse(res);
    }
}