namespace InChambers.Core.Utilities;

public static class Extensions
{
    public static string FormatDuration(TimeSpan duration)
    {
        if (duration.TotalHours >= 1)
        {
            return duration.Hours > 0 ? $"{duration.Hours}hr {duration.Minutes}m" : $"{duration.TotalMinutes}m";
        }
        else if (duration.TotalMinutes >= 1)
        {
            return $"{duration.Minutes}m";
        }
        else
        {
            return $"{duration.Seconds}s";
        }
    }
}
