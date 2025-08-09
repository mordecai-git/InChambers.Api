using InChambers.Core.Models.Input.Series;
using InChambers.Core.Models.Input.Videos;
using InChambers.Core.Models.Utilities;

namespace InChambers.Core.Interfaces;

public interface ISeriesService
{
    Task<Result> CreateSeries(SeriesModel model);
    Task<Result> UpdateSeries(string seriesUid, SeriesModel model);
    Task<Result> DeleteSeries(string seriesUid);
    Task<Result> GetSeries(string seriesUid);
    Task<Result> ListSeries(SeriesSearchModel request);
    Task<Result> AddSeriesView(string seriesUid);
    Task<Result> PublishSeries(string seriesUid);
    Task<Result> ActivateSeries(string seriesUid);
    Task<Result> DeactivateSeries(string seriesUid);
    Task<Result> GetUploadToken(string seriesUid);
    Task<Result> SetPreviewDetails(string seriesUid, VideoDetailModel model);
    Task<Result> DeletePreviewVideo(string seriesUid);
    Task<Result> GetPreviewDetails(string seriesUid);
    Task<Result> AddExistingCourseToSeries(string seriesUid, string courseUid);
    Task<Result> AddNewCourseToSeries(string seriesUid, SeriesNewCourseModel model);
    Task<Result> RemoveCourseFromSeries(string seriesUid, string seriesCourseId);
    Task<Result> ListCoursesInSeries(string seriesUid);
    Task<Result> ChangeCourseOrder(string seriesUid, string courseUid, SeriesCourseOrderModel model);
}