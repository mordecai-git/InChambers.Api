namespace InChambers.Core.Models.App.Constants;

public static class CourseAuditLogConstants
{
    public static string Created(string title, string fullName) => $"Course '{title}' was created by {fullName}";

    public static string Updated(string title, string fullName) => $"Course '{title}' was updated by {fullName}";

    public static string Deleted(string title, string fullName) => $"Course '{title}' was deleted by {fullName}";

    public static string Published(string title, string fullName) => $"Course '{title}' was published by {fullName}";

    public static string Activated(string title, string fullName) => $"Course '{title}' was activated by {fullName}";

    public static string Deactivated(string title, string fullName) => $"Course '{title}' was deactivated by {fullName}";

    public static string AddResource(string title, string fullName, string documentName) => $"Resource: '{documentName}' was added to course '{title}' by {fullName}";

    public static string RemoveResource(string title, string fullName, string documentName) => $"Resource: '{documentName}' was removed from course '{title}' by {fullName}";

}