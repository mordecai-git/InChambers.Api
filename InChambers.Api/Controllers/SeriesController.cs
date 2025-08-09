using InChambers.Core.Interfaces;
using InChambers.Core.Models.ApiVideo.Response;
using InChambers.Core.Models.Input.Series;
using InChambers.Core.Models.Input.Videos;
using InChambers.Core.Models.Utilities;
using InChambers.Core.Models.View;
using InChambers.Core.Models.View.Series;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InChambers.Api.Controllers
{
    [ApiController]
    [Route("api/v1/series")]
    [Authorize]
    public class SeriesController : BaseController
    {
        private readonly ISeriesService _seriesService;

        public SeriesController(ISeriesService seriesService)
            => _seriesService = seriesService ?? throw new ArgumentNullException(nameof(seriesService));

        /// <summary>
        /// Create a new series
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(SuccessResult<SeriesView>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
        public async Task<IActionResult> CreateSeries(SeriesModel model)
        {
            var res = await _seriesService.CreateSeries(model);
            return ProcessResponse(res);
        }

        /// <summary>
        /// Update series details
        /// </summary>
        /// <param name="seriesUid"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("{seriesUid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResult<SeriesView>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
        public async Task<IActionResult> UpdateSeries(string seriesUid, SeriesModel model)
        {
            var res = await _seriesService.UpdateSeries(seriesUid, model);
            return ProcessResponse(res);
        }

        /// <summary>
        /// Delete a series
        /// </summary>
        /// <param name="seriesUid"></param>
        /// <returns></returns>
        [HttpGet("{seriesUid}/delete")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResult))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
        public async Task<IActionResult> DeleteSeries(string seriesUid)
        {
            var res = await _seriesService.DeleteSeries(seriesUid);
            return ProcessResponse(res);
        }

        /// <summary>
        /// List all series
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResult<List<SeriesView>>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
        public async Task<IActionResult> ListSeries([FromQuery] SeriesSearchModel model)
        {
            var res = await _seriesService.ListSeries(model);
            return ProcessResponse(res);
        }

        /// <summary>
        /// Get series details
        /// </summary>
        /// <param name="seriesUid"></param>
        /// <returns></returns>
        [HttpGet("{seriesUid}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResult<SeriesDetailView>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
        public async Task<IActionResult> GetSeries(string seriesUid)
        {
            var res = await _seriesService.GetSeries(seriesUid);
            return ProcessResponse(res);
        }

        /// <summary>
        /// Add a view to a series
        /// </summary>
        /// <param name="seriesUid"></param>
        /// <returns></returns>
        [HttpPost("{seriesUid}/view")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResult))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
        public async Task<IActionResult> ViewSeries(string seriesUid)
        {
            var res = await _seriesService.AddSeriesView(seriesUid);
            return ProcessResponse(res);
        }

        /// <summary>
        /// Publish a series
        /// </summary>
        /// <param name="seriesUid"></param>
        /// <returns></returns>
        [HttpPost("{seriesUid}/publish")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResult))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
        public async Task<IActionResult> PublishSeries(string seriesUid)
        {
            var res = await _seriesService.PublishSeries(seriesUid);
            return ProcessResponse(res);
        }

        /// <summary>
        /// Activate a series
        /// </summary>
        /// <param name="seriesUid"></param>
        /// <returns></returns>
        [HttpPost("{seriesUid}/activate")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResult))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
        public async Task<IActionResult> ActivateSeries(string seriesUid)
        {
            var res = await _seriesService.ActivateSeries(seriesUid);
            return ProcessResponse(res);
        }

        /// <summary>
        /// Deactivate a series
        /// </summary>
        /// <param name="seriesUid"></param>
        /// <returns></returns>
        [HttpPost("{seriesUid}/deactivate")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResult))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
        public async Task<IActionResult> DeactivateSeries(string seriesUid)
        {
            var res = await _seriesService.DeactivateSeries(seriesUid);
            return ProcessResponse(res);
        }

        /// <summary>
        /// Get upload token for a series
        /// </summary>
        /// <param name="seriesUid"></param>
        /// <returns></returns>
        [HttpPost("{seriesUid}/preview-token")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResult<ApiVideoToken>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
        public async Task<IActionResult> GetPreviewToken(string seriesUid)
        {
            var res = await _seriesService.GetUploadToken(seriesUid);
            return ProcessResponse(res);
        }

        /// <summary>
        /// Set preview details for a series
        /// </summary>
        /// <param name="seriesUid"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("{seriesUid}/preview")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResult))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
        public async Task<IActionResult> SetPreview(string seriesUid, VideoDetailModel model)
        {
            var res = await _seriesService.SetPreviewDetails(seriesUid, model);
            return ProcessResponse(res);
        }

        /// <summary>
        /// Delete preview details for a series
        /// </summary>
        /// <param name="seriesUid"></param>
        /// <returns></returns>
        [HttpGet("{seriesUid}/preview/delete")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResult))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
        public async Task<IActionResult> DeletePreview(string seriesUid)
        {
            var res = await _seriesService.DeletePreviewVideo(seriesUid);
            return ProcessResponse(res);
        }

        /// <summary>
        /// Get preview details for a series
        /// </summary>
        /// <param name="seriesUid"></param>
        /// <returns></returns>
        [HttpGet("{seriesUid}/preview")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResult<VideoView>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
        public async Task<IActionResult> GetPreview(string seriesUid)
        {
            var res = await _seriesService.GetPreviewDetails(seriesUid);
            return ProcessResponse(res);
        }

        /// <summary>
        /// Add a new course to a series
        /// </summary>
        /// <param name="seriesUid"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("{seriesUid}/courses")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResult<SeriesCourseView>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
        public async Task<IActionResult> AddNewCourseToSeries(string seriesUid, SeriesNewCourseModel model)
        {
            var res = await _seriesService.AddNewCourseToSeries(seriesUid, model);
            return ProcessResponse(res);
        }

        /// <summary>
        /// Add an existing course to a series
        /// </summary>
        /// <param name="seriesUid"></param>
        /// <param name="courseUid"></param>
        /// <returns></returns>
        [HttpPost("{seriesUid}/courses/{courseUid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResult<SeriesCourseView>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
        public async Task<IActionResult> AddCourseToSeries(string seriesUid, string courseUid)
        {
            var res = await _seriesService.AddExistingCourseToSeries(seriesUid, courseUid);
            return ProcessResponse(res);
        }

        /// <summary>
        /// Remove a course from a series
        /// </summary>
        /// <param name="seriesUid"></param>
        /// <param name="seriesCourseId"></param>
        /// <returns></returns>
        [HttpGet("{seriesUid}/courses/{seriesCourseId}/delete")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResult))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
        public async Task<IActionResult> RemoveCourseFromSeries(string seriesUid, string seriesCourseId)
        {
            var res = await _seriesService.RemoveCourseFromSeries(seriesUid, seriesCourseId);
            return ProcessResponse(res);
        }

        /// <summary>
        /// List all courses in a series
        /// </summary>
        /// <param name="seriesUid"></param>
        /// <returns></returns>
        [HttpGet("{seriesUid}/courses")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResult<List<SeriesCourseView>>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
        public async Task<IActionResult> ListCoursesInSeries(string seriesUid)
        {
            var res = await _seriesService.ListCoursesInSeries(seriesUid);
            return ProcessResponse(res);
        }

        /// <summary>
        /// Change the order of a course in a series
        /// </summary>
        /// <param name="seriesUid"></param>
        /// <param name="courseUid"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("{seriesUid}/courses/{courseUid}/order")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResult))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
        public async Task<IActionResult> ChangeCourseOrder(string seriesUid, string courseUid, SeriesCourseOrderModel model)
        {
            var res = await _seriesService.ChangeCourseOrder(seriesUid, courseUid, model);
            return ProcessResponse(res);
        }
    }
}