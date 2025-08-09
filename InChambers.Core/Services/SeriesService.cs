using Mapster;
using InChambers.Core.Extensions;
using InChambers.Core.Interfaces;
using InChambers.Core.Models.ApiVideo.Response;
using InChambers.Core.Models.App;
using InChambers.Core.Models.App.Constants;
using InChambers.Core.Models.Input.Auth;
using InChambers.Core.Models.Input.Series;
using InChambers.Core.Models.Input.Videos;
using InChambers.Core.Models.Utilities;
using InChambers.Core.Models.View;
using InChambers.Core.Models.View.Series;
using InChambers.Core.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace InChambers.Core.Services;

public class SeriesService : ISeriesService
{
    private readonly InChambersContext _context;
    private readonly UserSession _userSession;
    private readonly IVideoService _videoService;

    public SeriesService(InChambersContext context, UserSession userSession, IVideoService videoService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _userSession = userSession ?? throw new ArgumentNullException(nameof(userSession));
        _videoService = videoService ?? throw new ArgumentNullException(nameof(videoService));
    }

    public async Task<Result> CreateSeries(SeriesModel model)
    {
        bool seriesExists = await _context.Series
            .AnyAsync(x => x.Title.ToLower().Trim() == model.Title.ToLower().Trim());

        if (seriesExists)
        {
            // return validation error

        }
        //return new ErrorResult(StatusCodes.Status400BadRequest, "Series with the same title already exists.");

        // create new series object
        var series = model.Adapt<Series>();
        series.CreatedById = _userSession.UserId;
        series.Uid = GetSeriesUid(model.Title);
        series.Preview = new()
        {
            UploadToken = ""
        };

        // add prices
        if (model.Prices.Any())
        {
            series.Prices = model.Prices.Select(p => new SeriesPrice
            {
                Price = p.Price,
                DurationId = p.DurationId
            }).ToList();
        }

        // add audit log
        AddSeriesAuditLog(series, SeriesAuditLogConstants.Created(series.Title, _userSession.Name));

        // save the data
        await _context.AddAsync(series);
        int saved = await _context.SaveChangesAsync();

        return saved > 0
            ? new SuccessResult(StatusCodes.Status201Created, series.Adapt<SeriesDetailView>())
            : new ErrorResult("Failed to create series.");
    }

    public async Task<Result> UpdateSeries(string seriesUid, SeriesModel model)
    {
        var series = await _context.Series
            .Include(x => x.Prices)
            .FirstOrDefaultAsync(x => x.Uid == seriesUid);

        if (series == null)
            return new ErrorResult("Series not found.");

        // update series object
        series.Title = model.Title;
        //series.Uid = await GetSeriesUid(model.Title);
        series.UpdatedById = _userSession.UserId;
        series.UpdatedOnUtc = DateTime.UtcNow;

        // remove already deleted prices
        var removedPrices = series.Prices
            .Where(x => model.Prices.All(p => p.DurationId != x.DurationId));
        _context.RemoveRange(removedPrices);

        // add new prices
        foreach (var price in model.Prices)
        {
            if (series.Prices.Any(x => x.DurationId == price.DurationId))
            {
                series.Prices.First(x => x.DurationId == price.DurationId).Price = price.Price;
            }
            else
            {
                series.Prices.Add(new SeriesPrice
                {
                    Price = price.Price,
                    DurationId = price.DurationId
                });
            }
        }

        // add audit log
        AddSeriesAuditLog(series, SeriesAuditLogConstants.Updated(series.Title, _userSession.Name));

        // save the data
        _context.Update(series);
        int saved = await _context.SaveChangesAsync();

        return saved > 0
            ? new SuccessResult(StatusCodes.Status200OK, series.Adapt<SeriesDetailView>())
            : new ErrorResult("Failed to update series.");
    }

    public async Task<Result> DeleteSeries(string seriesUid)
    {
        var series = await _context.Series
            .FirstOrDefaultAsync(x => x.Uid == seriesUid);

        if (series == null)
            return new ErrorResult(StatusCodes.Status404NotFound, "Series not found.");

        // TODO: Allow garbage collector to delete the video from API.Video using the background task
        // delete preview video
        //var preview = await _context.SeriesPreviews
        //    .FirstOrDefaultAsync(x => x.SeriesId == series.Id);
        //if (preview is not null && !string.IsNullOrEmpty(preview.VideoId))
        //{
        //    var deletedRes = await _videoService.DeleteVideo(series.Preview.VideoId);
        //    if (deletedRes.Success)
        //    {
        //        preview.VideoId = null;
        //        preview.ThumbnailUrl = null;
        //        preview.VideoDuration = 0;
        //        preview.IsUploaded = false;
        //    }
        //    else
        //    {
        //        Log.Error("Failed to delete preview video for series: {SeriesUid}, videoId: {VideoId}", seriesUid, series.Preview.VideoId);
        //    }
        //}

        // TODO: Allow garbage collector to delete the video from API.Video using the background task
        // delete videos that belong to only this series
        //var courseVideoIds = _context.SeriesCourses
        //    .Where(sc => sc.Course!.ForSeriesOnly && sc.SeriesId == series.Id)
        //    .Select(sc => sc.Course!.Video!.VideoId);

        //foreach (var videoId in courseVideoIds)
        //{
        //    var deletedRes = await _videoService.DeleteVideo(videoId!);
        //    if (!deletedRes.Success)
        //        Log.Error("Failed to delete existing video for series: {SeriesUid}, videoId: {VideoId}", seriesUid, videoId);
        //}

        // delete the series
        _context.Remove(series);

        // add audit log
        AddSeriesAuditLog(series, SeriesAuditLogConstants.Deleted(series.Title, _userSession.Name));

        int saved = await _context.SaveChangesAsync();

        return saved > 0
            ? new SuccessResult(StatusCodes.Status200OK, "S")
            : new ErrorResult("Failed to delete series.");
    }

    public async Task<Result> ListSeries(SeriesSearchModel request)
    {
        request.SearchQuery = !string.IsNullOrWhiteSpace(request.SearchQuery)
            ? request.SearchQuery.ToLower().Trim()
            : null;

        var series = _context.Series
            .Where(s => !s.IsDeleted).AsQueryable();

        // allow filters only for admin users or users who can manage courses
        series = _userSession.IsAuthenticated && (_userSession.IsAnyAdmin || _userSession.InRole(RolesConstants.ManageCourse))
            ? series.Where(s => !request.IsActive.HasValue || s.IsActive == request.IsActive)
            : series.Where(s => s.IsActive && s.IsPublished && !s.IsDeleted);

        var result = await series
            .Where(s => string.IsNullOrWhiteSpace(request.SearchQuery) ||
                        s.Title.ToLower().Contains(request.SearchQuery) || s.Summary.ToLower().Contains(request.SearchQuery))
            .Select(s => new SeriesView
            {
                Uid = s.Uid,
                Title = s.Title,
                Summary = s.Summary,
                CreatedById = s.CreatedById,
                IsActive = s.IsActive,
                IsPublished = s.IsPublished,
                CreatedAtUtc = s.CreatedAtUtc,
                PublishedOnUtc = s.PublishedOnUtc,
                Preview = s.Preview.Adapt<VideoView>(),
                Duration = Utilities.Extensions.FormatDuration(TimeSpan.FromSeconds(s.Courses
                    .Where(c => !c.IsDeleted)
                    .Select(c => c.Course!.Video != null ? c.Course.Video.VideoDuration : 0)
                    .Sum())),
                HasBought = _userSession.IsAuthenticated && s.UserSeries.Any(us => us.UserId == _userSession.UserId && !us.IsExpired)
            }).ToPaginatedListAsync(request.PageIndex, request.PageSize);

        return new SuccessResult(result);
    }

    public async Task<Result> GetSeries(string seriesUid)
    {
        var series = _context.Series.AsQueryable();

        // include deleted ones if user is admin
        if (!_userSession.IsAnyAdmin && !_userSession.IsCourseManager)
            series = series.Where(s => !s.IsDeleted && s.IsActive && s.IsPublished);

        var result = await series
            .Where(s => s.Uid == seriesUid)
            .ProjectToType<SeriesDetailView>()
            .FirstOrDefaultAsync();

        if (result is null)
            return new ErrorResult(StatusCodes.Status404NotFound, "Series is not found.");

        // remove the deleted prices
        result.Prices = result.Prices.Where(x => !x.IsDeleted).ToList();

        return new SuccessResult(result);
    }

    public async Task<Result> AddSeriesView(string seriesUid)
    {
        var series = await _context.Series
            .FirstOrDefaultAsync(x => x.Uid == seriesUid);

        if (series == null)
            return new ErrorResult("Series not found.");

        series.ViewCount++;
        _context.Update(series);
        int saved = await _context.SaveChangesAsync();

        if (saved < 1)
            Log.Error(
                "An error occurred while updating series view count for series: {SeriesTitle}. Current view count before failure: {Count}",
                series.Title, series.ViewCount - 1);

        return saved > 0
            ? new SuccessResult(StatusCodes.Status200OK, "Series view count updated successfully.")
            : new ErrorResult("Failed to add view.");
    }

    public async Task<Result> PublishSeries(string seriesUid)
    {
        var series = await _context.Series
            .Where(s => s.Uid == seriesUid)
            .Include(s => s.Preview)
            .FirstOrDefaultAsync();

        if (series is null)
            return new ErrorResult(StatusCodes.Status404NotFound, "Series is not found.");

        if (series.IsPublished)
            return new ErrorResult("Series is already published.");

        // validate that series set up is complete
        if (string.IsNullOrWhiteSpace(series.Preview!.VideoId))
            return new ErrorResult("Series preview video is not set.");

        series.IsPublished = true;
        series.IsActive = true;
        series.PublishedOnUtc = DateTime.UtcNow;
        series.PublishedById = _userSession.UserId;

        _context.Update(series);

        // add audit log
        AddSeriesAuditLog(series, SeriesAuditLogConstants.Published(series.Title, _userSession.Name));

        int saved = await _context.SaveChangesAsync();

        return saved > 0
            ? new SuccessResult(StatusCodes.Status200OK, "Series published successfully.")
            : new ErrorResult("Failed to publish series.");
    }

    public async Task<Result> ActivateSeries(string seriesUid)
    {
        var series = await _context.Series
            .Where(s => s.Uid == seriesUid)
            .FirstOrDefaultAsync();

        if (series is null)
            return new ErrorResult(StatusCodes.Status404NotFound, "Series is not found.");

        series.IsActive = true;

        // add audit log
        AddSeriesAuditLog(series, SeriesAuditLogConstants.Activated(series.Title, _userSession.Name));

        int saved = await _context.SaveChangesAsync();

        return saved > 0
            ? new SuccessResult(StatusCodes.Status200OK, "Series activated successfully.")
            : new ErrorResult("Failed to activate series.");
    }

    public async Task<Result> DeactivateSeries(string seriesUid)
    {
        var series = await _context.Series
            .Where(s => s.Uid == seriesUid)
            .FirstOrDefaultAsync();

        if (series is null)
            return new ErrorResult(StatusCodes.Status404NotFound, "Series is not found.");

        series.IsActive = false;

        // add audit log
        AddSeriesAuditLog(series, SeriesAuditLogConstants.Deactivated(series.Title, _userSession.Name));

        int saved = await _context.SaveChangesAsync();

        return saved > 0
            ? new SuccessResult(StatusCodes.Status200OK, "Series deactivated successfully.")
            : new ErrorResult("Failed to deactivate series.");
    }

    public async Task<Result> GetUploadToken(string seriesUid)
    {
        var preview = _context.SeriesPreviews
            .Where(cv => cv.Series!.Uid == seriesUid)
            .FirstOrDefault();

        if (preview is null || string.IsNullOrEmpty(preview.UploadToken))
        {
            var uploadTokenRes = await _videoService.GetUploadToken();
            if (!uploadTokenRes.Success)
                return new ErrorResult(uploadTokenRes.Message);

            var uploadToken = uploadTokenRes.Content;

            int seriesId = await _context.Series
                .Where(c => c.Uid == seriesUid)
                .Select(c => c.Id)
                .FirstOrDefaultAsync();

            if (preview is not null)
            {
                preview.UploadToken = uploadToken!.Token;

                _context.Update(preview);
            }
            else
            {
                var newUploadData = new SeriesPreview
                {
                    SeriesId = seriesId,
                    UploadToken = uploadToken!.Token
                };
                await _context.AddAsync(newUploadData);
            }

            await _context.SaveChangesAsync();
            return new SuccessResult(uploadToken);
        }

        return new SuccessResult(new ApiVideoToken { Token = preview.UploadToken });
    }

    public async Task<Result> SetPreviewDetails(string seriesUid, VideoDetailModel model)
    {
        var series = await _context.Series
            .Where(c => c.Uid == seriesUid)
            .Include(series => series.Preview)
            .Select(c => new Series
            {
                Id = c.Id,
                Preview = c.Preview
            })
            .FirstOrDefaultAsync();

        if (series is null)
            return new ErrorResult(StatusCodes.Status404NotFound, "Series not found.");

        // delete existing video
        if (!string.IsNullOrEmpty(series.Preview!.VideoId))
        {
            var deletedRes = await _videoService.DeleteVideo(series.Preview.VideoId);
            if (!deletedRes.Success)
                Log.Error("Failed to delete existing video for series: {SeriesUid}, videoId: {VideoId}", seriesUid, series.Preview.VideoId);
        }

        model.@public = true;
        var detailsSet = await _videoService.SetVideoDetails(model);
        if (!detailsSet.Success)
            return detailsSet;

        series.Preview!.VideoId = model.videoId;
        series.Preview.ThumbnailUrl = model.ThumbnailUrl;
        series.Preview.IsUploaded = true;
        series.Preview.VideoDuration = model.Duration;

        int saved = await _context.SaveChangesAsync();

        return saved > 0
            ? new SuccessResult()
            : new ErrorResult("Failed to save video details to server.");
    }

    public async Task<Result> DeletePreviewVideo(string seriesUid)
    {
        var preview = await _context.Series
            .Where(c => c.Uid == seriesUid)
            .Include(series => series.Preview)
            .Select(c => c.Preview)
            .FirstOrDefaultAsync();

        if (preview is null)
            return new ErrorResult(StatusCodes.Status404NotFound, "Series not found.");

        // delete video
        if (!string.IsNullOrEmpty(preview!.VideoId))
        {
            var deletedRes = await _videoService.DeleteVideo(preview.VideoId);
            if (!deletedRes.Success)
            {
                Log.Error("Failed to delete existing video for series: {SeriesUid}, videoId: {VideoId}", seriesUid, preview.VideoId);
                return deletedRes;
            }
        }

        preview.VideoId = null;
        preview.ThumbnailUrl = null;
        preview.VideoDuration = 0;
        preview.IsUploaded = false;

        _context.Update(preview);

        int saved = await _context.SaveChangesAsync();

        return saved > 0
            ? new SuccessResult()
            : new ErrorResult("Failed to delete video details.");
    }

    public async Task<Result> GetPreviewDetails(string seriesUid)
    {
        var previewDetails = await _context.SeriesPreviews
            .Where(cv => cv.Series!.Uid == seriesUid)
            .ProjectToType<VideoView>()
            .FirstOrDefaultAsync();

        if (previewDetails is null)
            return new SuccessResult(StatusCodes.Status204NoContent, "Preview details not found.");

        return new SuccessResult(previewDetails);
    }

    public async Task<Result> AddExistingCourseToSeries(string seriesUid, string courseUid)
    {
        var series = await _context.Series
            .FirstOrDefaultAsync(x => x.Uid == seriesUid);

        if (series is null)
            return new ErrorResult(StatusCodes.Status404NotFound, "Series not found.");

        var course = await _context.Courses
            .Where(c => c.Uid == courseUid)
            .Select(c => new Course { Id = c.Id, Uid = c.Uid, IsActive = c.IsActive, Title = c.Title, Summary = c.Summary })
            .FirstOrDefaultAsync();

        if (course is null || course.IsDeleted)
            return new ErrorResult(StatusCodes.Status404NotFound, "Course not found or cannot be added because it has been deleted.");
        if (!course.IsActive)
            return new ErrorResult("Cannot add a deactivated course.");

        bool courseExistInSeries = await _context.SeriesCourses
            .AnyAsync(sc => sc.SeriesId == series.Id && sc.CourseId == course.Id && !sc.IsDeleted);

        if (courseExistInSeries)
            return new ErrorResult("Course already exists in this series.");

        var seriesCourse = new SeriesCourse
        {
            SeriesId = series.Id,
            CourseId = course.Id,
            Order = _context.SeriesCourses.Count(sc => sc.SeriesId == series.Id && !sc.IsDeleted) + 1,
            CreatedById = _userSession.UserId
        };

        await _context.AddAsync(seriesCourse);

        int saved = await _context.SaveChangesAsync();

        if (saved < 1)
            return new ErrorResult("Failed to save changes.");

        seriesCourse.Course = course;
        return new SuccessResult(seriesCourse.Adapt<SeriesCourseView>());
    }

    public async Task<Result> AddNewCourseToSeries(string seriesUid, SeriesNewCourseModel model)
    {
        var series = await _context.Series
            .FirstOrDefaultAsync(x => x.Uid == seriesUid);

        if (series is null)
            return new ErrorResult(StatusCodes.Status404NotFound, "Series not found.");

        var newCourse = model.Adapt<Course>();
        newCourse.CreatedById = _userSession.UserId;
        newCourse.Uid = await GetCourseUid(model.Title);
        newCourse.ForSeriesOnly = true;
        newCourse.IsPublished = true;

        // set video details
        var detailsSet = await _videoService.SetVideoDetails(model.VideoDetails);
        if (!detailsSet.Success)
            return detailsSet;

        newCourse.Video = new CourseVideo
        {
            UploadToken = "",
            VideoId = model.VideoDetails.videoId,
            VideoDuration = model.VideoDetails.Duration,
            IsUploaded = true
        };

        var seriesCourse = new SeriesCourse
        {
            SeriesId = series.Id,
            Course = newCourse,
            Order = _context.SeriesCourses.Count(sc => sc.SeriesId == series.Id && !sc.IsDeleted) + 1,
            CreatedById = _userSession.UserId
        };

        await _context.AddAsync(seriesCourse);

        int saved = await _context.SaveChangesAsync();

        return saved > 0
            ? new SuccessResult(seriesCourse.Adapt<SeriesCourseView>())
            : new ErrorResult("Failed to save changes.");
    }

    public async Task<Result> RemoveCourseFromSeries(string seriesUid, string seriesCourseId)
    {
        var seriesCourse = await _context.SeriesCourses
            .Include(sc => sc.Course).ThenInclude(c => c.Video)
            .FirstOrDefaultAsync(sc => sc.Course!.Uid == seriesCourseId && sc.Series!.Uid == seriesUid);

        if (seriesCourse is null)
            return new ErrorResult(StatusCodes.Status404NotFound, "Course not found in this series.");

        // TODO: Allow garbage collector to delete the video from API.Video using the background task
        // Remove video if it belongs to this series only
        //if (seriesCourse.Course!.ForSeriesOnly)
        //{
        //    if (!string.IsNullOrEmpty(seriesCourse.Course.Video!.VideoId))
        //    {
        //        var deletedRes = await _videoService.DeleteVideo(seriesCourse.Course.Video.VideoId);
        //        if (deletedRes.Success)
        //        {
        //            seriesCourse.Course.Video.VideoId = null;
        //            seriesCourse.Course.Video.ThumbnailUrl = null;
        //            seriesCourse.Course.Video.VideoDuration = 0;
        //            seriesCourse.Course.Video.IsUploaded = false;
        //        }
        //        else
        //        {
        //            Log.Error("Failed to delete existing video for series course: {SeriesCourseId}, videoId: {VideoId}", seriesCourseId, seriesCourse.Course.Video.VideoId);
        //        }
        //    }
        //}

        _context.Remove(seriesCourse);

        // get courses after this one
        var remainingCourses = await _context.SeriesCourses
            .Where(sc => sc.SeriesId == seriesCourse.SeriesId && sc.Order > seriesCourse.Order)
            .ToListAsync();

        // decrement the order of the remaining courses
        foreach (var course in remainingCourses)
        {
            course.Order--;
            _context.Update(course);
        }

        int saved = await _context.SaveChangesAsync();

        return saved > 0
            ? new SuccessResult()
            : new ErrorResult("Failed to delete series course.");
    }

    public async Task<Result> ListCoursesInSeries(string seriesUid)
    {
        var seriesCourses = await _context.SeriesCourses
            .Include(sc => sc.Course)
            .Where(sc => sc.Series!.Uid == seriesUid && !sc.IsDeleted)
            .OrderBy(sc => sc.Order)
            .ProjectToType<SeriesCourseView>()
            .ToListAsync();

        return new SuccessResult(seriesCourses);
    }

    public async Task<Result> ChangeCourseOrder(string seriesUid, string courseUid, SeriesCourseOrderModel model)
    {
        var seriesCourse = await _context.SeriesCourses
            .Where(sc => sc.Series!.Uid == seriesUid && sc.Course!.Uid == courseUid && !sc.IsDeleted)
            .FirstOrDefaultAsync();
        if (seriesCourse is null)
            return new ErrorResult("Course not found in this series.");

        // Get the current order of the course
        int currentOrder = seriesCourse.Order;

        // Update the order of the specified course
        seriesCourse.Order = model.NewOrder;
        seriesCourse.UpdatedById = _userSession.UserId;
        seriesCourse.UpdatedOnUtc = DateTime.UtcNow;

        // Adjust the order of the other courses in the series
        var otherCourses = await _context.SeriesCourses
            .Where(sc => sc.Series!.Uid == seriesUid && sc.Course!.Uid != courseUid && !sc.IsDeleted)
            .ToListAsync();

        foreach (var course in otherCourses)
        {
            if (model.NewOrder < currentOrder)
            {
                if (course.Order >= model.NewOrder && course.Order < currentOrder)
                    course.Order++;
            }
            else
            {
                if (course.Order <= model.NewOrder && course.Order > currentOrder)
                    course.Order--;
            }

            course.UpdatedById = _userSession.UserId;
            course.UpdatedOnUtc = DateTime.UtcNow;
            _context.Update(course);
        }

        _context.Update(seriesCourse);

        int saved = await _context.SaveChangesAsync();

        return saved > 0
            ? new SuccessResult()
            : new ErrorResult("Failed to save changes.");
    }

    #region PRIVATE METHODS

    private static string GetSeriesUid(string title)
    {
        var trimmedTitle = title.Trim() // trim
            .ToLower().Replace("-", "", StringComparison.OrdinalIgnoreCase) // remove hyphens
            .Replace(" ", "-", StringComparison.OrdinalIgnoreCase); // replace spaces with hyphens

        // get the next series number from sequence
        //var nextSeriesNumber = await _context.GetNextSeriesNumber();
        //return $"{trimmedTitle}-{nextSeriesNumber}";
        return $"{trimmedTitle}";
    }

    private async Task<string> GetCourseUid(string title)
    {
        var trimmedTitle = title.Trim()  // trim
            .ToLower().Replace("-", "", StringComparison.OrdinalIgnoreCase) // remove hyphens
            .Replace(" ", "-", StringComparison.OrdinalIgnoreCase); // replace spaces with hyphens

        // get the next course number from sequence
        var nextCourseNumber = await _context.GetNextCourseNumber();
        return $"{trimmedTitle}-{nextCourseNumber}";
    }

    private async void AddSeriesAuditLog(Series Series, string description)
    {
        var newLog = new SeriesAuditLog
        {
            Series = Series,
            Description = description,
            CreatedById = _userSession.UserId
        };
        await _context.AddAsync(newLog);
    }

    #endregion
}