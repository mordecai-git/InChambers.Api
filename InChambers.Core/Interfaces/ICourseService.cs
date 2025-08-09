using InChambers.Core.Models.Input;
using InChambers.Core.Models.Input.Courses;
using InChambers.Core.Models.Input.Videos;
using InChambers.Core.Models.Utilities;

namespace InChambers.Core.Interfaces;

public interface ICourseService
{
    Task<Result> CreateCourse(CourseModel model);
    Task<Result> UpdateCourse(string courseUid, CourseModel model);
    Task<Result> DeleteCourse(string courseUid);
    Task<Result> GetCourse(string courseUid);
    Task<Result> AddCourseView(string courseUid);
    Task<Result> ListCourses(CourseSearchModel request);
    Task<Result> PublishCourse(string courseUid);
    Task<Result> ActivateCourse(string courseUid);
    Task<Result> DeactivateCourse(string courseUid);
    Task<Result> SetCourseVideoDetails(string courseUid, VideoDetailModel model);
    Task<Result> AddResourceToCourse(string courseUid, FileUploadModel file);
    Task<Result> RemoveResourceFromCourse(string courseUid, int documentId);
    Task<Result> ListResources(string courseUid);
}