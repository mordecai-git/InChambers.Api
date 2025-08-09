using InChambers.Core.Models.App;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace InChambers.Core.Utilities;

internal static class DatabaseExtensions
{
    public static async Task<int> GetNextCourseNumber(this InChambersContext context)
        => await GetNextNumber(context, "CourseNumber");

    public static async Task<int> GetNextSeriesNumber(this InChambersContext context)
        => await GetNextNumber(context, "SeriesNumber");


    private static async Task<int> GetNextNumber(InChambersContext context, string type)
    {
        var sqlParameter = new SqlParameter("@result", SqlDbType.Int)
        {
            Direction = ParameterDirection.Output
        };
        await context.Database
            .ExecuteSqlRawAsync($"set @result = next value for {type}", sqlParameter);
        int nextVal = (int)sqlParameter.Value;
        return nextVal;
    }
}
