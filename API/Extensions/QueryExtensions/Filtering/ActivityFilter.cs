using System.Linq;
using API.DTOs.Statistics;
using API.Entities.Progress;

namespace API.Extensions.QueryExtensions.Filtering;

public static class ActivityFilter
{

    public static IQueryable<AppUserReadingSessionActivityData> ApplyStatsFilter(
        this IQueryable<AppUserReadingSessionActivityData> queryable,
        StatsFilterDto filter,
        int userId,
        bool onlyCompleted = true
        )
    {
        var startTime = filter.StartDate?.ToUniversalTime();
        var endTime = filter.EndDate?.ToUniversalTime();

        return queryable
            .Where(d => filter.Libraries.Contains(d.LibraryId) && d.ReadingSession.AppUserId == userId)
            .WhereIf(onlyCompleted, d => d.EndPage >= d.Chapter.Pages)
            .WhereIf(startTime != null, d => d.StartTime >= startTime)
            .WhereIf(endTime != null, d => d.EndTime <= endTime);
    }

}
