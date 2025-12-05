using System.Linq;
using API.DTOs.Statistics;
using API.Entities;
using API.Entities.Enums;
using API.Entities.Enums.UserPreferences;
using API.Entities.Progress;
using API.Entities.User;

namespace API.Extensions.QueryExtensions.Filtering;

public static class ActivityFilter
{

    /// <summary>
    /// Filter AppUserReadingSessionActivityData for the given filter, viewer, and owner
    /// </summary>
    /// <param name="queryable">source</param>
    /// <param name="filter">stats filter from the UI</param>
    /// <param name="userId">user id of the user <b>owing</b> the data</param>
    /// <param name="socialPreferences">social preferences of the user <b>owing</b> the data</param>
    /// <param name="appUser">the user <b>requesting</b> the data</param>
    /// <param name="onlyCompleted">return only data for fully read chapters</param>
    /// <returns></returns>
    public static IQueryable<AppUserReadingSessionActivityData> ApplyStatsFilter(
        this IQueryable<AppUserReadingSessionActivityData> queryable,
        StatsFilterDto filter,
        int userId,
        AppUserSocialPreferences  socialPreferences,
        AppUser appUser,
        bool onlyCompleted = true
        )
    {
        var startTime = filter.StartDate?.ToUniversalTime();
        var endTime = filter.EndDate?.ToUniversalTime();
        var isOwnRequest = userId == appUser.Id;

        return queryable
            .Where(d => filter.Libraries.Contains(d.LibraryId) && d.ReadingSession.AppUserId == userId)
            .WhereIf(!isOwnRequest && socialPreferences.SocialLibraries.Count > 0, d => socialPreferences.SocialLibraries.Contains(d.LibraryId))
            .WhereIf(!isOwnRequest && socialPreferences.SocialMaxAgeRating != AgeRating.NotApplicable, d =>
                (socialPreferences.SocialMaxAgeRating >= d.Chapter.Volume.Series.Metadata.AgeRating && d.Chapter.Volume.Series.Metadata.AgeRating != AgeRating.Unknown)
                || (socialPreferences.SocialIncludeUnknowns && d.Chapter.Volume.Series.Metadata.AgeRating == AgeRating.Unknown )
                )
            .WhereIf(!isOwnRequest && appUser.AgeRestriction != AgeRating.NotApplicable, d =>
                (appUser.AgeRestriction >= d.Chapter.Volume.Series.Metadata.AgeRating && d.Chapter.Volume.Series.Metadata.AgeRating != AgeRating.Unknown)
                || (appUser.AgeRestrictionIncludeUnknowns && d.Chapter.Volume.Series.Metadata.AgeRating == AgeRating.Unknown )
                )
            .WhereIf(onlyCompleted, d => d.EndPage >= d.Chapter.Pages)
            .WhereIf(startTime != null, d => d.StartTime >= startTime)
            .WhereIf(endTime != null, d => d.EndTime <= endTime);
    }

}
