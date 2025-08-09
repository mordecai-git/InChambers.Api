namespace InChambers.Core.Models.App.Constants;

public static class SeriesAuditLogConstants
{
    public static string Created(string title, string fullName) => $"Series '{title}' was created by {fullName}";

    public static string Updated(string title, string fullName) => $"Series '{title}' was updated by {fullName}";

    public static string Deleted(string title, string fullName) => $"Series '{title}' was deleted by {fullName}";

    public static string Published(string title, string fullName) => $"Series '{title}' was published by {fullName}";

    public static string Activated(string title, string fullName) => $"Series '{title}' was activated by {fullName}";

    public static string Deactivated(string title, string fullName) => $"Series '{title}' was deactivated by {fullName}";
}