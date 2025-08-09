using System.Text;
using Mapster;
using InChambers.Core.Constants;
using InChambers.Core.Interfaces;
using InChambers.Core.Models.ApiVideo.Response;
using InChambers.Core.Models.App;
using InChambers.Core.Models.Configurations;
using InChambers.Core.Models.Input.Auth;
using InChambers.Core.Models.Input.Courses;
using InChambers.Core.Models.Input.Videos;
using InChambers.Core.Models.Utilities;
using InChambers.Core.Models.View;
using InChambers.Core.Models.View.Series;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;

namespace InChambers.Core.Services;

public class VideoService : IVideoService
{
    private readonly HttpClient _client;
    private readonly ApiVideoConfig _apiVideoConfig;
    private readonly InChambersContext _context;
    private readonly UserSession _userSession;
    private readonly ILogger _logger;

    public VideoService(IHttpClientFactory factory, IOptions<AppConfig> appConfig, InChambersContext context, UserSession userSession, ILogger logger)
    {
        if (appConfig is null) throw new ArgumentNullException(nameof(appConfig));
        if (factory is null) throw new ArgumentNullException(nameof(factory));
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _userSession = userSession ?? throw new ArgumentNullException(nameof(userSession));

        _apiVideoConfig = appConfig.Value.ApiVideo;
        _client = factory.CreateClient(HttpClientKeys.ApiVideo);
    }

    public async Task<Result<ApiVideoToken>> GetUploadToken(int expiresInSec = 0)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        if (expiresInSec == 0)
        {
            response = await _client.PostAsync("upload-tokens", null);
        }
        else
        {
            var request = new { ttl = expiresInSec };
            var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            response = await _client.PostAsync("upload-tokens", content);
        }

        if (!response.IsSuccessStatusCode)
            return new ErrorResult<ApiVideoToken>("Failed to get video upload token");

        string contentString = await response.Content.ReadAsStringAsync();
        var uploadToken = JsonConvert.DeserializeObject<ApiVideoToken>(contentString);

        return new SuccessResult<ApiVideoToken>(uploadToken!);
    }

    public async Task<Result> GetVideoUploadData(string courseUid)
    {
        var uploadToken = _context.CourseVideos
            .Where(cv => cv.Course!.Uid == courseUid)
            .Select(cv => new ApiVideoToken { Token = cv.UploadToken })
            .FirstOrDefault();

        if (uploadToken is null)
        {
            var response = await _client.PostAsync("upload-tokens", null);
            if (!response.IsSuccessStatusCode)
                return new ErrorResult("Failed to get video upload token");

            string content = await response.Content.ReadAsStringAsync();
            uploadToken = JsonConvert.DeserializeObject<ApiVideoToken>(content);

            int courseId = await _context.Courses
                .Where(c => c.Uid == courseUid)
                .Select(c => c.Id)
                .FirstOrDefaultAsync();

            var newUploadData = new CourseVideo
            {
                CourseId = courseId,
                UploadToken = uploadToken!.Token
            };
            await _context.AddAsync(newUploadData);
            await _context.SaveChangesAsync();
        }

        return new SuccessResult(uploadToken);
    }

    public async Task<Result> SetVideoDetails(VideoDetailModel model)
    {
        model.playerId = _apiVideoConfig.PlayerId;
        var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

        var response = await _client.PatchAsync($"videos/{model.videoId}", content);
        if (response.IsSuccessStatusCode)
        {
            return new SuccessResult();
        }

        string contentString = await response.Content.ReadAsStringAsync();
        object error = JsonConvert.DeserializeObject<object>(contentString);
        _logger.Error("Failed to set video details. {@Error}", contentString);
        return new ErrorResult("Failed to set video details.");
    }

    public async Task<Result> SetVideoPreview(string courseUid, ApiVideoClipModel model)
    {
        var courseVideo = await _context.CourseVideos
            .Where(cv => cv.Course!.Uid == courseUid)
            .Include(cv => cv.Course)
            .FirstOrDefaultAsync();

        if (courseVideo is null || !courseVideo.IsUploaded)
            return new ErrorResult(StatusCodes.Status404NotFound, "Source video information not found.");

        // check if there is a preview video already set
        if (!string.IsNullOrWhiteSpace(courseVideo.PreviewVideoId))
        {
            var deleteResponse = await _client.DeleteAsync($"videos/{courseVideo.PreviewVideoId}");
            if (!deleteResponse.IsSuccessStatusCode)
            {
                string contentString = await deleteResponse.Content.ReadAsStringAsync();
                object error = JsonConvert.DeserializeObject<object>(contentString);
                _logger.Error("Failed to delete video preview. {@Error}", error);
                return new ErrorResult("Failed to delete existing video preview.");
            }
        }

        var request = new
        {
            source = courseVideo.VideoId!,
            title = courseVideo.Course!.Title + "- (Preview)",
            @public = true,
            mp4Support = false,
            playerId = _apiVideoConfig.PlayerId,
            clip = new
            {
                startTimecode = model.startTimecode.ToString(),
                endTimecode = model.endTimecode.ToString()
            }
        };

        var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("videos", content);
        if (!response.IsSuccessStatusCode)
        {
            string contentString = await response.Content.ReadAsStringAsync();
            object error = JsonConvert.DeserializeObject<object>(contentString);
            _logger.Error("Failed to set video preview. {@Error}", error);
            return new ErrorResult("Failed to set video thumbnail.");
        }

        string responseContent = await response.Content.ReadAsStringAsync();
        var apiResult = JsonConvert.DeserializeObject<ApiVideoDetail>(responseContent);

        courseVideo.PreviewVideoId = apiResult!.VideoId;
        courseVideo.ThumbnailUrl = apiResult!.Assets.Thumbnail;
        courseVideo.PreviewStart = model.startTimecode;
        courseVideo.PreviewEnd = model.endTimecode;

        int saved = await _context.SaveChangesAsync();

        return saved > 0
            ? new SuccessResult(courseVideo.Adapt<VideoView>())
            : new ErrorResult("Failed to save video thumbnail to server.");
    }

    public async Task<Result> DeleteVideo(string videoId)
    {
        var response = await _client.DeleteAsync($"videos/{videoId}");
        if (response.IsSuccessStatusCode)
        {
            return new SuccessResult();
        }

        string contentString = await response.Content.ReadAsStringAsync();
        object error = JsonConvert.DeserializeObject<object>(contentString);
        _logger.Error("Failed to delete video. {@Error}", error);
        return new ErrorResult("Failed to delete video.");
    }

    public async Task<Result> GetVideoPlayerDetails(string courseUid)
    {
        // validate user has paid for course if not a super admin, admin or course manager
        bool shouldHaveUnrestrictedAccess = _userSession.IsAnyAdmin || _userSession.IsCourseManager;
        if (!shouldHaveUnrestrictedAccess)
        {
            var coursePaid = await _context.UserCourses
                .Where(cp => cp.Course!.Uid == courseUid && cp.UserId == _userSession.UserId)
                .Where(cp => !cp.IsExpired)
                .AnyAsync();
            if (!coursePaid)
                return new ForbiddenResult();
        }

        var courseVideo = await _context.CourseVideos
            .Where(cv => cv.Course!.Uid == courseUid)
            .FirstOrDefaultAsync();

        if (courseVideo is null || !courseVideo.IsUploaded)
            return new SuccessResult(StatusCodes.Status204NoContent, "Video information not found.");

        string videoId = courseVideo.VideoId!;
        if (string.IsNullOrWhiteSpace(videoId) && !shouldHaveUnrestrictedAccess)
            return new ErrorResult("Video not uploaded.");

        var response = await _client.GetAsync($"videos/{videoId}");
        if (!response.IsSuccessStatusCode)
            return new ErrorResult("Failed to get video details.");

        string content = await response.Content.ReadAsStringAsync();
        var apiResult = JsonConvert.DeserializeObject<ApiVideoDetail>(content);

        // extract the token from the result
        string token = apiResult!.Assets.Player.Split("?token=").Last();

        var result = new VideoView
        {
            VideoId = videoId,
            Token = token,
            PreviewVideoId = courseVideo.PreviewVideoId,
            ThumbnailUrl = courseVideo.ThumbnailUrl,
            VideoDuration = courseVideo.VideoDuration,
            IsUploaded = courseVideo.IsUploaded
        };

        return new SuccessResult(result);
    }

    public async Task<Result> GetUserCourseProgress(string courseUid)
    {
        // validate user has paid for course
        var courseProgress = await _context.UserCourses
            .Where(cp => cp.Course!.Uid == courseUid && cp.UserId == _userSession.UserId)
            .Where(cp => !cp.IsExpired)
            .FirstOrDefaultAsync();

        if (courseProgress is null)
            return new ForbiddenResult();

        var courseVideo = await _context.CourseVideos
            .Where(cv => cv.Course!.Uid == courseUid)
            .FirstOrDefaultAsync();

        if (courseVideo is null || !courseVideo.IsUploaded)
            return new SuccessResult(StatusCodes.Status204NoContent, "Video information not found.");

        string videoId = courseVideo.VideoId!;
        if (string.IsNullOrWhiteSpace(videoId))
            return new ErrorResult("Video not uploaded.");

        var response = await _client.GetAsync($"videos/{videoId}");
        if (!response.IsSuccessStatusCode)
            return new ErrorResult("Failed to get video details.");

        string content = await response.Content.ReadAsStringAsync();
        var apiResult = JsonConvert.DeserializeObject<ApiVideoDetail>(content);

        // extract the token from the result
        string token = apiResult!.Assets.Player.Split("?token=").Last();

        var result = new VideoView
        {
            VideoId = videoId,
            Token = token,
            PreviewVideoId = courseVideo.PreviewVideoId,
            ThumbnailUrl = courseVideo.ThumbnailUrl,
            VideoDuration = courseVideo.VideoDuration,
            Progress = courseProgress.Progress
        };

        return new SuccessResult(result);
    }

    public async Task<Result> ReportCourseProgress(string courseUid, ProgressReportModel model)
    {
        var courseProgress = await _context.UserCourses
               .Where(cp => cp.Course!.Uid == courseUid && cp.UserId == _userSession.UserId)
               .Where(cp => !cp.IsExpired)
               .FirstOrDefaultAsync();

        if (courseProgress is null)
            return new ForbiddenResult();

        courseProgress.Progress = model.Progress;

        _context.UserCourses.Update(courseProgress);

        int saved = await _context.SaveChangesAsync();

        return saved > 0
            ? new SuccessResult()
            : new ErrorResult("Unable to save video progress");
    }

    public async Task<Result> CourseVideoCompleted(string courseUid)
    {
        var courseProgress = await _context.UserCourses
            .Where(cp => cp.Course!.Uid == courseUid && cp.UserId == _userSession.UserId)
            .Where(cp => !cp.IsExpired)
            .FirstOrDefaultAsync();

        if (courseProgress is null)
            return new ForbiddenResult();

        courseProgress.IsCompleted = true;

        _context.UserCourses.Update(courseProgress);

        int saved = await _context.SaveChangesAsync();

        return saved > 0
            ? new SuccessResult()
            : new ErrorResult("Unable to save video progress");
    }

    public async Task<Result> GetUserSeriesProgress(string seriesUid)
    {
        var userSeries = await _context.UserSeries
            .Where(sp => sp.Series!.Uid == seriesUid && sp.UserId == _userSession.UserId)
            .Where(sp => !sp.IsExpired)
            .FirstOrDefaultAsync();

        if (userSeries is null)
            return new ErrorResult(StatusCodes.Status404NotFound, "The resource you requested for cannot be found.");

        var progress = _context.SeriesProgress
            .Where(sp => sp.UserSeriesId == userSeries.Id)
            .Select(sp => new SeriesProgressView
            {
                CourseUid = sp.Course!.Uid,
                Order = sp.Order,
                Progress = sp.Progress,
                IsCompleted = sp.IsCompleted
            });

        return new SuccessResult(progress);
    }

    public async Task<Result> GetSeriesCourseVideoDetail(string seriesUid, string courseUid)
    {
        var userSeries = await _context.UserSeries
            .Where(sp => sp.Series!.Uid == seriesUid && sp.UserId == _userSession.UserId)
            .Where(sp => !sp.IsExpired)
            .FirstOrDefaultAsync();

        if (userSeries is null)
            return new ErrorResult(StatusCodes.Status404NotFound, "The resource you requested for cannot be found.");

        var seriesProgress = await _context.SeriesProgress
            .Where(sp => sp.UserSeriesId == userSeries.Id)
            .Where(sp => sp.Course!.Uid == courseUid)
            .FirstOrDefaultAsync();

        if (seriesProgress is null)
            return new ErrorResult(StatusCodes.Status404NotFound, "The resource you requested for cannot be found.");

        // validate that the user has finished the previous video if any
        var previousCourse = await _context.SeriesProgress
            .Where(sp => sp.UserSeriesId == userSeries.Id)
            .Where(sp => sp.Order == seriesProgress.Order - 1)
            .FirstOrDefaultAsync();

        // if the video is the first in the series, then it can be completed
        if (previousCourse is null && seriesProgress.Order == 1 || !previousCourse!.IsCompleted)
            return new ErrorResult(StatusCodes.Status303SeeOther, "Previous course not completed");

        var courseVideo = await _context.CourseVideos
            .Where(cv => cv.Course!.Uid == courseUid)
            .FirstOrDefaultAsync();

        if (courseVideo is null || !courseVideo.IsUploaded)
            return new SuccessResult(StatusCodes.Status204NoContent, "Video information not found.");

        string videoId = courseVideo.VideoId!;

        var response = await _client.GetAsync($"videos/{videoId}");
        if (!response.IsSuccessStatusCode)
            return new ErrorResult("Failed to get video details.");

        string content = await response.Content.ReadAsStringAsync();
        var apiResult = JsonConvert.DeserializeObject<ApiVideoDetail>(content);

        // extract the token from the result
        string token = apiResult!.Assets.Player.Split("?token=").Last();

        var result = new VideoView
        {
            VideoId = videoId,
            Token = token,
            PreviewVideoId = courseVideo.PreviewVideoId,
            ThumbnailUrl = courseVideo.ThumbnailUrl,
            VideoDuration = courseVideo.VideoDuration,
            Progress = seriesProgress.Progress
        };

        return new SuccessResult(result);
    }

    public async Task<Result> ReportSeriesCourseProgress(string seriesUid, string courseUid, ProgressReportModel model)
    {
        var userSeries = await _context.UserSeries
            .Where(sp => sp.Series!.Uid == seriesUid && sp.UserId == _userSession.UserId)
            .Where(sp => !sp.IsExpired)
            .FirstOrDefaultAsync();

        if (userSeries is null)
            return new ErrorResult(StatusCodes.Status404NotFound, "The resource you requested for cannot be found.");

        var seriesProgress = await _context.SeriesProgress
            .Where(sp => sp.UserSeriesId == userSeries.Id)
            .Where(sp => sp.Course!.Uid == courseUid)
            .FirstOrDefaultAsync();

        if (seriesProgress is null)
            return new ErrorResult(StatusCodes.Status404NotFound, "The resource you requested for cannot be found.");

        seriesProgress.Progress = model.Progress;

        _context.SeriesProgress.Update(seriesProgress);

        int saved = await _context.SaveChangesAsync();

        return saved > 0
            ? new SuccessResult()
            : new ErrorResult("Unable to save video progress");
    }

    public async Task<Result> SeriesCourseVideoCompleted(string seriesUid, string courseUid)
    {
        var userSeries = await _context.UserSeries
            .Where(sp => sp.Series!.Uid == seriesUid && sp.UserId == _userSession.UserId)
            .Where(sp => !sp.IsExpired)
            .FirstOrDefaultAsync();

        if (userSeries is null)
            return new ErrorResult(StatusCodes.Status404NotFound, "The resource you requested for cannot be found.");

        var seriesProgress = await _context.SeriesProgress
            .Where(sp => sp.UserSeriesId == userSeries.Id)
            .Where(sp => sp.Course!.Uid == courseUid)
            .FirstOrDefaultAsync();

        if (seriesProgress is null)
            return new ErrorResult(StatusCodes.Status404NotFound, "The resource you requested for cannot be found.");

        seriesProgress.IsCompleted = true;

        _context.SeriesProgress.Update(seriesProgress);

        // check if video is the last in the series and if all other course has been completed
        var seriesProgresses = await _context.SeriesProgress
            .Where(sp => sp.UserSeriesId == userSeries.Id)
            .ToListAsync();

        bool allCompleted = seriesProgresses.All(sp => sp.IsCompleted);

        if (allCompleted)
        {
            userSeries.IsCompleted = true;
            _context.UserSeries.Update(userSeries);
        }
        else
        {
            // get the first un completed course if the last is already completed
            var nextCourse = seriesProgresses
                .Where(sp => !sp.IsCompleted)
                .OrderBy(sp => sp.Order)
                .FirstOrDefault();
        }

        int saved = await _context.SaveChangesAsync();

        return saved > 0
            ? new SuccessResult()
            : new ErrorResult("Unable to save video progress");
    }
}