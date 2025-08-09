using InChambers.Core.Models.ApiVideo.Response;
using InChambers.Core.Models.Input.Courses;
using InChambers.Core.Models.Input.Videos;
using InChambers.Core.Models.Utilities;

namespace InChambers.Core.Interfaces;

public interface IVideoService
{
    Task<Result<ApiVideoToken>> GetUploadToken(int expiresInSec = 0);
    Task<Result> GetVideoUploadData(string courseUid);
    Task<Result> SetVideoDetails(VideoDetailModel model);
    Task<Result> SetVideoPreview(string courseUid, ApiVideoClipModel model);
    Task<Result> DeleteVideo(string videoId);
    Task<Result> GetVideoPlayerDetails(string courseUid);
    Task<Result> GetUserCourseProgress(string courseUid);
    Task<Result> ReportCourseProgress(string courseUid, ProgressReportModel model);
    Task<Result> CourseVideoCompleted(string courseUid);
    Task<Result> GetUserSeriesProgress(string seriesUid);
    Task<Result> GetSeriesCourseVideoDetail(string seriesUid, string courseUid);
    Task<Result> ReportSeriesCourseProgress(string seriesUid, string courseUid, ProgressReportModel model);
    Task<Result> SeriesCourseVideoCompleted(string seriesUid, string courseUid);
}