using Mapster;
using InChambers.Core.Extensions;
using InChambers.Core.Interfaces;
using InChambers.Core.Models.App;
using InChambers.Core.Models.App.Constants;
using InChambers.Core.Models.Input;
using InChambers.Core.Models.Input.Auth;
using InChambers.Core.Models.Input.Courses;
using InChambers.Core.Models.Input.Videos;
using InChambers.Core.Models.Utilities;
using InChambers.Core.Models.View;
using InChambers.Core.Models.View.Courses;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace InChambers.Core.Services;

public class CourseService : ICourseService
{
    private readonly InChambersContext _context;
    private readonly UserSession _userSession;
    private readonly IFileService _fileService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IVideoService _videoService;

    public CourseService(InChambersContext context, UserSession userSession, IFileService fileService,
        IHttpContextAccessor httpContextAccessor, IVideoService videoService)
    {
        _context = context ?? throw new ArgumentException(nameof(context));
        _userSession = userSession ?? throw new ArgumentException(nameof(userSession));
        _fileService = fileService ?? throw new ArgumentException(nameof(fileService));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentException(nameof(httpContextAccessor));
        _videoService = videoService ?? throw new ArgumentException(nameof(videoService));
    }

    #region Courses

    public async Task<Result> CreateCourse(CourseModel model)
    {
        // validate that course with title doesn't exist
        bool courseExist = await _context.Courses
            .Where(c => !c.ForSeriesOnly)
            .AnyAsync(c => c.Title.ToLower().Trim() == model.Title.ToLower().Trim());

        if (courseExist)
            return new ErrorResult("A course with this title already exist. Please choose another title.");

        // create course object
        var course = model.Adapt<Course>();
        course.CreatedById = _userSession.UserId;
        course.Uid = GetCourseUid(model.Title);

        // add prices
        if (model.Prices.Any())
        {
            course.Prices = model.Prices.Select(p => new CoursePrice
            {
                Price = p.Price,
                DurationId = p.DurationId
            }).ToList();
        }

        // add audit log
        AddCourseAuditLog(course, CourseAuditLogConstants.Created(course.Title, _userSession.Name));

        // save the data
        await _context.AddAsync(course);
        int saved = await _context.SaveChangesAsync();

        return saved > 0
            ? new SuccessResult(StatusCodes.Status201Created, course.Adapt<CourseView>())
            : new ErrorResult("Failed to create course. Please try again.");
    }

    public async Task<Result> UpdateCourse(string courseUid, CourseModel model)
    {
        // get the course
        var course = await _context.Courses
            .Where(c => !c.ForSeriesOnly)
            .Include(c => c.Prices)
            .Include(c => c.UsefulLinks)
            .Where(c => c.Uid == courseUid)
            .FirstOrDefaultAsync();

        if (course == null)
            return new ErrorResult("Course not found.");

        // validate that course with title doesn't exist
        bool courseExist = await _context.Courses
            .Where(c => !c.ForSeriesOnly)
            .AnyAsync(c => c.Id != course.Id
                           && c.Title.ToLower().Trim() == model.Title.ToLower().Trim());

        if (courseExist)
            return new ErrorResult("A course with this title already exist. Please choose another title.");

        // update the course object
        course.Title = model.Title;
        course.Summary = model.Summary;
        course.Description = model.Description;
        course.Tags = string.Join(",", model.Tags);
        //course.Uid = await GetCourseUid(model.Title);
        course.UpdatedById = _userSession.UserId;
        course.UpdatedOnUtc = DateTime.UtcNow;
        course.UsefulLinks = new();

        // update the prices
        foreach (var price in model.Prices)
        {
            var existingPrice = course.Prices.FirstOrDefault(cp => cp.DurationId == price.DurationId);
            if (existingPrice is not null)
            {
                existingPrice.Price = price.Price;
                existingPrice.DurationId = price.DurationId;
            }
            else
            {
                course.Prices.Add(new CoursePrice
                {
                    Price = price.Price,
                    DurationId = price.DurationId
                });
            }
        }

        // remove the already deleted useful links
        var removedLinks = course.UsefulLinks
            .Where(ul => model.UsefulLinks.All(l => ul.Title != l.Title && ul.Link != l.Link));
        _context.CourseLinks.RemoveRange(removedLinks);

        // add new links
        foreach (var link in model.UsefulLinks)
        {
            var existingLink = course.UsefulLinks.FirstOrDefault(ul => ul.Title == link.Title && ul.Link == link.Link);
            if (existingLink is null)
            {
                course.UsefulLinks.Add(new CourseLink
                {
                    Title = link.Title,
                    Link = link.Link
                });
            }
        }

        // save the data
        _context.Update(course);
        int saved = await _context.SaveChangesAsync();

        return saved > 0
            ? new SuccessResult(course.Adapt<CourseDetailView>())
            : new ErrorResult("Failed to update course. Please try again.");
    }

    public async Task<Result> DeleteCourse(string courseUid)
    {
        // get the course
        var course = await _context.Courses
            .Where(c => !c.ForSeriesOnly)
            .Where(c => c.Uid == courseUid)
            .FirstOrDefaultAsync();
        if (course == null)
            return new ErrorResult(StatusCodes.Status404NotFound, "Course not found.");

        // TODO: Implement garbage collector to delete the video from API.Video using the background task

        // delete the course
        _context.Remove(course);

        // add audit log
        AddCourseAuditLog(course, CourseAuditLogConstants.Deleted(course.Title, _userSession.Name));

        int deleted = await _context.SaveChangesAsync();

        return deleted > 0
            ? new SuccessResult("Course deleted successfully.")
            : new ErrorResult("Failed to delete course. Please try again.");
    }

    public async Task<Result> GetCourse(string courseUid)
    {
        var course = _context.Courses
            .Where(c => !c.ForSeriesOnly)
            .AsQueryable();

        // include deleted ones if user is admin
        if (!_userSession.IsAnyAdmin && !_userSession.IsCourseManager)
            course = course.Where(c => !c.IsDeleted && c.IsActive && c.IsPublished);

        var result = await course
            .Where(c => c.Uid == courseUid)
            .ProjectToType<CourseDetailView>()
            .FirstOrDefaultAsync();

        if (result is null)
            return new ErrorResult(StatusCodes.Status404NotFound, "Course not found.");

        // remove deleted prices
        result.Prices = result.Prices.Where(cp => !cp.IsDeleted).ToList();

        result.Resources = _context.CourseDocuments
            .Where(cd => cd.CourseId == result.Id)
            .Select(cd => cd.Document)
            .ProjectToType<DocumentView>();

        return new SuccessResult(result);
    }

    public async Task<Result> AddCourseView(string courseUid)
    {
        var course = await _context.Courses
            .Where(c => !c.ForSeriesOnly)
            .Where(c => c.Uid == courseUid)
            .FirstOrDefaultAsync();
        if (course == null)
            return new ErrorResult(StatusCodes.Status404NotFound, "Course not found.");

        course.ViewCount++;
        _context.Update(course);

        var countDetail = new CourseViewCount
        {
            CourseId = course.Id,
            ViewedById = _userSession.UserId != 0 ? _userSession.UserId : null,
            IpAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString()
        };
        await _context.AddRangeAsync(countDetail);

        int saved = await _context.SaveChangesAsync();

        if (saved < 1)
            Log.Error(
                "An error occurred while updating course view count for course: {CourseTitle}. Current view count before failure: {Count}",
                course.Title, course.ViewCount - 1);

        return saved > 0
            ? new SuccessResult(StatusCodes.Status200OK, "Course view count updated successfully.")
            : new ErrorResult("Failed to update course view count. Please try again.");
    }

    public async Task<Result> ListCourses(CourseSearchModel request)
    {
        request.SearchQuery = !string.IsNullOrEmpty(request.SearchQuery)
            ? request.SearchQuery.ToLower().Trim()
            : null;

        var courses = _context.Courses
            .Where(c => !c.IsDeleted && !c.ForSeriesOnly).AsQueryable();

        // allow filters only for admin users or users who can manage courses
        courses = _userSession.IsAuthenticated && (_userSession.IsAnyAdmin || _userSession.InRole(RolesConstants.ManageCourse))
            ? courses
                .Where(c => !request.IsActive.HasValue || c.IsActive == request.IsActive)
            : courses.Where(c => !c.IsDeleted && c.IsActive && c.IsPublished);

        // TODO: implement Full text search for description and title
        var today = DateTime.UtcNow.Date;
        var result = await courses
            .Where(c => string.IsNullOrEmpty(request.SearchQuery)
                        || c.Title.ToLower().Contains(request.SearchQuery) || c.Summary.ToLower().Contains(request.SearchQuery))
            .Include(c => c.Video)
            .OrderBy(c => c.Title).ThenBy(c => c.Summary)
            .Select(c => new CourseView
            {
                Uid = c.Uid,
                Title = c.Title,
                Summary = c.Summary,
                CreatedById = c.CreatedById,
                IsPublished = c.IsPublished,
                IsActive = c.IsActive,
                CreatedAtUtc = c.CreatedAtUtc,
                PublishedOnUtc = c.PublishedOnUtc,
                ThumbnailUrl = c.Video != null ? c.Video.ThumbnailUrl : null,
                Prices = c.Prices.Where(cp => !cp.IsDeleted).Select(cp => new PriceView
                {
                    Price = cp.Price,
                    Name = cp.Duration!.Name
                }).ToList(),
                Duration = c.Video != null ? TimeSpan.FromSeconds(c.Video.VideoDuration).ToString("hh\\:mm\\:ss") : null,
                HasBought = _userSession.IsAuthenticated && c.UserCourses.Any(o => o.CourseId == c.Id && o.UserId == _userSession.UserId && !o.IsExpired)
            })
            .ToPaginatedListAsync(request.PageIndex, request.PageSize);

        return new SuccessResult(result);
    }

    public async Task<Result> PublishCourse(string courseUid)
    {
        // get the course
        var course = await _context.Courses
            .Where(c => !c.ForSeriesOnly)
            .Where(c => c.Uid == courseUid)
            .Include(c => c.Video)
            .FirstOrDefaultAsync();
        if (course == null)
            return new ErrorResult(StatusCodes.Status404NotFound, "Course not found.");

        if (course.IsPublished)
            return new ErrorResult("Course is already published.");

        if (!course.Video!.IsUploaded)
            return new ErrorResult("Course video is not uploaded yet. Please upload the video first.");

        course.IsPublished = true;
        course.IsActive = true;
        course.PublishedOnUtc = DateTime.UtcNow;
        course.PublishedById = _userSession.UserId;

        _context.Update(course);

        // add audit log
        AddCourseAuditLog(course, CourseAuditLogConstants.Published(course.Title, _userSession.Name));

        int published = await _context.SaveChangesAsync();

        return published > 0
            ? new SuccessResult(StatusCodes.Status200OK, "Course published successfully.")
            : new ErrorResult("Failed to publish course. Please try again.");
    }

    public async Task<Result> ActivateCourse(string courseUid)
    {
        // get the course
        var course = await _context.Courses
            .Where(c => !c.ForSeriesOnly)
            .Where(c => c.Uid == courseUid)
            .FirstOrDefaultAsync();
        if (course == null)
            return new ErrorResult(StatusCodes.Status404NotFound, "Course not found.");

        course.IsActive = true;

        _context.Update(course);

        // add audit log
        AddCourseAuditLog(course, CourseAuditLogConstants.Activated(course.Title, _userSession.Name));

        int activated = await _context.SaveChangesAsync();

        return activated > 0
            ? new SuccessResult(StatusCodes.Status200OK, "Course activated successfully.")
            : new ErrorResult("Failed to activate course. Please try again.");
    }

    public async Task<Result> DeactivateCourse(string courseUid)
    {
        // get the course
        var course = await _context.Courses
            .Where(c => !c.ForSeriesOnly)
            .Where(c => c.Uid == courseUid)
            .FirstOrDefaultAsync();
        if (course == null)
            return new ErrorResult(StatusCodes.Status404NotFound, "Course not found.");

        course.IsActive = false;

        _context.Update(course);

        // add audit log
        AddCourseAuditLog(course, CourseAuditLogConstants.Deactivated(course.Title, _userSession.Name));

        int deactivated = await _context.SaveChangesAsync();

        return deactivated > 0
            ? new SuccessResult(StatusCodes.Status200OK, "Course deactivated successfully.")
            : new ErrorResult("Failed to deactivate course. Please try again.");
    }

    #endregion

    #region Resources

    public async Task<Result> SetCourseVideoDetails(string courseUid, VideoDetailModel model)
    {
        var course = await _context.Courses
            .Where(c => !c.ForSeriesOnly)
            .Where(c => c.Uid == courseUid)
            .Include(course => course.Video)
            .Select(c => new Course
            {
                Id = c.Id,
                Video = c.Video
            })
            .FirstOrDefaultAsync();

        if (course is null)
            return new ErrorResult(StatusCodes.Status404NotFound, "Course not found.");

        // delete existing video
        if (course.Video is not null && course.Video.IsUploaded)
        {
            var videoDeleted = await _videoService.DeleteVideo(course.Video!.VideoId!);
            if (!videoDeleted.Success)
                return videoDeleted;
        }

        var detailsSet = await _videoService.SetVideoDetails(model);
        if (!detailsSet.Success)
            return detailsSet;

        course.Video!.VideoId = model.videoId;
        course.Video.IsUploaded = true;
        course.Video.VideoDuration = model.Duration;

        int saved = await _context.SaveChangesAsync();

        return saved > 0
            ? new SuccessResult()
            : new ErrorResult("Failed to save video details to server.");
    }

    public async Task<Result> AddResourceToCourse(string courseUid, FileUploadModel model)
    {
        // validate course
        var course = await _context.Courses
            .Where(c => !c.ForSeriesOnly)
            .Where(c => c.Uid == courseUid)
            .FirstOrDefaultAsync();

        if (course is null)
            return new ErrorResult("Invalid course selected.");

        // save the file
        var fileSaved = await _fileService.UploadFileInternal(course.Uid, model.File);
        if (!fileSaved.Success)
            return new ErrorResult(fileSaved.Title, fileSaved.Message);

        course.Resources.Add(new CourseDocument
        {
            Course = course,
            Document = fileSaved.Content,
            CreatedById = _userSession.UserId
        });

        // add audit log
        AddCourseAuditLog(course,
            CourseAuditLogConstants.AddResource(course.Title, _userSession.Name, fileSaved.Content.Name));

        _context.Update(course);
        int saved = await _context.SaveChangesAsync();

        return saved > 0
            ? new SuccessResult(StatusCodes.Status201Created, fileSaved.Content.Adapt<DocumentView>())
            : new ErrorResult("Failed to add resource to course. Please try again.");
    }

    public async Task<Result> RemoveResourceFromCourse(string courseUid, int documentId)
    {
        // validate document
        var courseDoc = await _context.CourseDocuments
            .Where(cd => cd.Course!.Uid == courseUid && cd.DocumentId == documentId)
            .Include(cd => cd.Course)
            .Include(cd => cd.Document)
            .FirstOrDefaultAsync();

        if (courseDoc is null)
            return new ErrorResult("Invalid resource.");

        // delete the file
        var fileDeleted = await _fileService.DeleteFileInternal(documentId);
        if (!fileDeleted.Success)
            return new ErrorResult(fileDeleted.Title, fileDeleted.Message);

        // remove the course document
        _context.Remove(courseDoc.Document!);
        _context.Remove(courseDoc);

        // add audit log
        AddCourseAuditLog(courseDoc.Course!,
            CourseAuditLogConstants.RemoveResource(courseDoc.Course!.Title, _userSession.Name,
                courseDoc.Document!.Name));

        int deleted = await _context.SaveChangesAsync();

        return deleted > 0
            ? new SuccessResult("Resource removed successfully.")
            : new ErrorResult("Failed to remove resource from course. Please try again.");
    }

    public async Task<Result> ListResources(string courseUid)
    {
        var resources = await _context.CourseDocuments
            .Where(cd => cd.Course!.Uid == courseUid)
            .Select(cd => cd.Document)
            .ProjectToType<DocumentView>()
            .ToListAsync();

        return new SuccessResult(resources);
    }

    #endregion

    #region Private Methods

    private static string GetCourseUid(string title)
    {
        var trimmedTitle = title.Trim()  // trim
            .ToLower().Replace("-", "", StringComparison.OrdinalIgnoreCase) // remove hyphens
            .Replace(" ", "-", StringComparison.OrdinalIgnoreCase); // replace spaces with hyphens

        // get the next course number from sequence
        //var nextCourseNumber = await _context.GetNextCourseNumber();
        //return $"{trimmedTitle}-{nextCourseNumber}";
        return $"{trimmedTitle}";
    }

    private async void AddCourseAuditLog(Course course, string description)
    {
        var newLog = new CourseAuditLog
        {
            Course = course,
            Description = description,
            CreatedById = _userSession.UserId
        };
        await _context.AddAsync(newLog);
    }

    #endregion
}