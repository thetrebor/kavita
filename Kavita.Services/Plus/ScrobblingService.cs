using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;
using Hangfire;
using Kavita.API.Database;
using Kavita.API.Repositories;
using Kavita.API.Services;
using Kavita.API.Services.Plus;
using Kavita.API.Services.SignalR;
using Kavita.Common;
using Kavita.Common.Helpers;
using Kavita.Models.DTOs.Filtering.v2;
using Kavita.Models.DTOs.Filtering.v2.Requests;
using Kavita.Models.DTOs.Scrobbling;
using Kavita.Models.DTOs.SignalR;
using Kavita.Models.Entities;
using Kavita.Models.Entities.Enums;
using Kavita.Models.Entities.Metadata;
using Kavita.Models.Entities.Scrobble;
using Kavita.Models.Entities.User;
using Kavita.Services.Plus.ScrobbleService;
using Kavita.Services.Scanner;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kavita.Services.Plus;

/// <summary>
/// Context used when syncing scrobble events. Do NOT reuse between syncs
/// </summary>
public class ScrobbleSyncContext
{
    public required List<ScrobbleEvent> ReadEvents {get; init;}
    public required List<ScrobbleEvent> RatingEvents {get; init;}
    /// <remarks>Do not use this as events to send to K+, use <see cref="Decisions"/></remarks>
    public required List<ScrobbleEvent> AddToWantToRead {get; init;}
    /// <remarks>Do not use this as events to send to K+, use <see cref="Decisions"/></remarks>
    public required List<ScrobbleEvent> RemoveWantToRead {get; init;}
    /// <summary>
    /// Final events list if all AddTo- and RemoveWantToRead would be processed sequentially
    /// </summary>
    public required List<ScrobbleEvent> Decisions {get; init;}
    /// <summary>
    /// K+ license
    /// </summary>
    public required string License { get; init; }
    /// <summary>
    /// Maps userId to left over request amount
    /// </summary>
    public required Dictionary<int, int> RateLimits { get; init; }

    /// <summary>
    /// All users being scrobbled for
    /// </summary>
    public List<AppUser> Users { get; set; } = [];
    /// <summary>
    /// Amount of already processed events
    /// </summary>
    public int ProgressCounter { get; set; }

    /// <summary>
    /// Sum of all events to process
    /// </summary>
    public int TotalCount => ReadEvents.Count + RatingEvents.Count + AddToWantToRead.Count + RemoveWantToRead.Count;
}

public class ScrobblingService : IScrobblingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventHub _eventHub;
    private readonly ILogger<ScrobblingService> _logger;
    private readonly ILicenseService _licenseService;
    private readonly ILocalizationService _localizationService;
    private readonly IEmailService _emailService;
    private readonly IKavitaPlusApiService _kavitaPlusApiService;
    private readonly IServiceProvider _serviceProvider;

    public const string AniListWeblinkWebsite = ScrobblingHelper.AniListWeblinkWebsite;
    public const string MalWeblinkWebsite = ScrobblingHelper.MalWeblinkWebsite;
    public const string MalStaffWebsite = ScrobblingHelper.MalStaffWebsite;
    public const string MalCharacterWebsite = ScrobblingHelper.MalCharacterWebsite;
    public const string GoogleBooksWeblinkWebsite = ScrobblingHelper.GoogleBooksWeblinkWebsite;
    public const string MangaDexWeblinkWebsite = ScrobblingHelper.MangaDexWeblinkWebsite;
    public const string AniListStaffWebsite = ScrobblingHelper.AniListStaffWebsite;
    public const string AniListCharacterWebsite = ScrobblingHelper.AniListCharacterWebsite;
    public const string HardcoverStaffWebsite = ScrobblingHelper.HardcoverStaffWebsite;

    private const int ScrobbleSleepTime = 1000; // We can likely tie this to AniList's 90 rate / min ((60 * 1000) / 90)
    private const SeriesIncludes ScrobbleSeriesIncludes = SeriesIncludes.Library | SeriesIncludes.ExternalMetadata | SeriesIncludes.Metadata;

    private static readonly IList<ScrobbleProvider> BookProviders = [
        ScrobbleProvider.Hardcover
    ];
    private static readonly IList<ScrobbleProvider> LightNovelProviders =
    [
        ScrobbleProvider.AniList, ScrobbleProvider.Hardcover
    ];
    private static readonly IList<ScrobbleProvider> ComicProviders = [
        ScrobbleProvider.Hardcover
    ];
    private static readonly IList<ScrobbleProvider> MangaProviders = [
        ScrobbleProvider.AniList, ScrobbleProvider.Hardcover, ScrobbleProvider.Mangabaka
    ];

    private const string UnknownSeriesErrorMessage = "Series cannot be matched for Scrobbling";
    private const string AccessTokenErrorMessage = "Access Token needs to be rotated to continue scrobbling";
    private const string InvalidKPlusLicenseErrorMessage = "Kavita+ subscription no longer active";
    private const string ReviewFailedErrorMessage = "Review was unable to be saved due to upstream requirements";
    private const string BadPayLoadErrorMessage = "Bad payload from Scrobble Provider";


    public ScrobblingService(IUnitOfWork unitOfWork, IEventHub eventHub, ILogger<ScrobblingService> logger,
        ILicenseService licenseService, ILocalizationService localizationService, IEmailService emailService,
        IKavitaPlusApiService kavitaPlusApiService, IServiceProvider serviceProvider)
    {
        _unitOfWork = unitOfWork;
        _eventHub = eventHub;
        _logger = logger;
        _licenseService = licenseService;
        _localizationService = localizationService;
        _emailService = emailService;
        _kavitaPlusApiService = kavitaPlusApiService;
        _serviceProvider = serviceProvider;

        FlurlConfiguration.ConfigureClientForUrl(Configuration.KavitaPlusApiUrl);
    }

    #region Access token checks

    /// <summary>
    /// An automated job that will run against all user's tokens and validate if they are still active
    /// </summary>
    /// <param name="ct"></param>
    /// <remarks>This service can validate without license check as the task which calls will be guarded</remarks>
    /// <returns></returns>
    public async Task CheckExternalAccessTokens(CancellationToken ct = default)
    {
        // Validate AniList
        var users = await _unitOfWork.UserRepository.GetAllUsersAsync(ct: ct);
        foreach (var user in users)
        {
            if (string.IsNullOrEmpty(user.AniListAccessToken)) continue;

            var tokenExpiry = JwtHelper.GetTokenExpiry(user.AniListAccessToken);

            // Send early reminder 5 days before token expiry
            if (await ShouldSendEarlyReminder(user.Id, tokenExpiry))
            {
                await _emailService.SendTokenExpiringSoonEmail(user.Id, ScrobbleProvider.AniList);
            }

            // Send expiration notification after token expiry
            if (await ShouldSendExpirationReminder(user.Id, tokenExpiry))
            {
                await _emailService.SendTokenExpiredEmail(user.Id, ScrobbleProvider.AniList);
            }

            // Check token validity
            if (JwtHelper.IsTokenValid(user.AniListAccessToken)) continue;

            _logger.LogInformation(
                "User {UserName}'s AniList token has expired or is expiring in a few days! They need to regenerate it for scrobbling to work",
                user.UserName);

            // Notify user via event
            await _eventHub.SendMessageToAsync(
                MessageFactory.ScrobblingKeyExpired,
                MessageFactory.ScrobblingKeyExpiredEvent(ScrobbleProvider.AniList),
                user.Id, ct);

        }
    }

    /// <summary>
    /// Checks if an early reminder email should be sent.
    /// </summary>
    private async Task<bool> ShouldSendEarlyReminder(int userId, DateTime tokenExpiry)
    {
        var earlyReminderDate = tokenExpiry.AddDays(-5);
        if (earlyReminderDate > DateTime.UtcNow) return false;

        var hasAlreadySentReminder = await _unitOfWork.DataContext.EmailHistory
            .AnyAsync(h => h.AppUserId == userId && h.Sent &&
                           h.EmailTemplate == EmailService.TokenExpiringSoonTemplate &&
                           h.SendDate >= earlyReminderDate);

        return !hasAlreadySentReminder;

    }

    /// <summary>
    /// Checks if an expiration notification email should be sent.
    /// </summary>
    private async Task<bool> ShouldSendExpirationReminder(int userId, DateTime tokenExpiry)
    {
        if (tokenExpiry > DateTime.UtcNow) return false;

        var hasAlreadySentExpirationEmail = await _unitOfWork.DataContext.EmailHistory
            .AnyAsync(h => h.AppUserId == userId && h.Sent &&
                           h.EmailTemplate == EmailService.TokenExpirationTemplate &&
                           h.SendDate >= tokenExpiry);

        return !hasAlreadySentExpirationEmail;
    }

    public async Task<bool> HasTokenExpired(int userId, ScrobbleProvider provider, CancellationToken ct = default)
    {
        var token = await GetTokenForProvider(userId, provider);

        if (await HasTokenExpired(token, provider))
        {
            // NOTE: Should this side effect be here?
            await _eventHub.SendMessageToAsync(MessageFactory.ScrobblingKeyExpired,
                MessageFactory.ScrobblingKeyExpiredEvent(ScrobbleProvider.AniList), userId, ct);
            return true;
        }

        return false;
    }

    private async Task<bool> HasTokenExpired(string token, ScrobbleProvider provider)
    {
        if (string.IsNullOrEmpty(token) || !TokenService.HasTokenExpired(token)) return false;

        var license = await _unitOfWork.SettingsRepository.GetSettingAsync(ServerSettingKey.LicenseKey);
        if (string.IsNullOrEmpty(license.Value)) return true;

        try
        {
            return await _kavitaPlusApiService.HasTokenExpiredAsync(license.Value, token, provider);
        }
        catch (HttpRequestException e)
        {
            _logger.LogError(e, "An error happened during the request to Kavita+ API");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error happened during the request to Kavita+ API");
        }

        return true;
    }

    private async Task<string> GetTokenForProvider(int userId, ScrobbleProvider provider)
    {
        var user = await _unitOfWork.UserRepository.GetUserByIdAsync(userId);
        if (user == null) return string.Empty;

        return provider switch
        {
            ScrobbleProvider.AniList => user.AniListAccessToken,
            _ => string.Empty
        } ?? string.Empty;
    }

    #endregion

    #region Scrobble ingest

    /// <summary>
    /// Returns the providers for which an
    /// </summary>
    /// <param name="scrobbleProviders">If not null, returned providers are guaranteed to be in this list</param>
    /// <param name="eventType"></param>
    /// <param name="user">Should include UserPreferences</param>
    /// <param name="series"></param>
    /// <returns></returns>
    private static List<ScrobbleProvider> GetProvidersForScrobbleEvent(
        List<ScrobbleProvider>? scrobbleProviders,
        ScrobbleEventType eventType,
        AppUser user,
        Series series)
    {
        var userPreferences = user.UserPreferences;

        Func<AppUserScrobbleSettings, bool> guard = eventType switch
        {
            ScrobbleEventType.ChapterRead => s => s.ProgressScrobbling,
            ScrobbleEventType.AddWantToRead => s => s.WantToReadSync,
            ScrobbleEventType.RemoveWantToRead => s => s.WantToReadSync,
            ScrobbleEventType.ScoreUpdated => s => s.RatingScrobbling,
            ScrobbleEventType.Review => s => s.ReviewsScrobbling,
            _ => throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null)
        };

        List<ScrobbleProvider> providers = [];

        foreach (var scrobbleProvider in Enum.GetValues<ScrobbleProvider>())
        {
            if (!userPreferences.ScrobbleSettings.TryGetValue(scrobbleProvider, out var settings) || !guard(settings))
            {
                continue;
            }

            if (!user.ScrobbleProviders.TryGetValue(scrobbleProvider, out var scrobbleProviderSettings)
                || string.IsNullOrEmpty(scrobbleProviderSettings.AuthenticationToken))
            {
                continue;
            }

            if (settings.HighestAgeRating != AgeRating.NotApplicable && series.Metadata.AgeRating > settings.HighestAgeRating)
            {
                continue;
            }

            if (!settings.AllLibraries && !settings.Libraries.Contains(series.LibraryId))
            {
                continue;
            }

            providers.Add(scrobbleProvider);
        }

        return scrobbleProviders == null ? providers : providers.Intersect(scrobbleProviders).ToList();
    }

    private async Task<(AppUser, Series, Chapter?)> LoadScrobbleEventData(int userId, int seriesId, int? chapterId,
        CancellationToken ct)
    {
        var series = await _unitOfWork.SeriesRepository.GetSeriesByIdAsync(seriesId, ScrobbleSeriesIncludes, ct);
        if (series == null) throw new KavitaException(await _localizationService.TranslateAsync(userId, "series-doesnt-exist"));

        Chapter? chapter = null;
        if (chapterId.HasValue)
        {
            chapter = await _unitOfWork.ChapterRepository.GetChapterAsync(chapterId.Value, ct: ct);
            if (chapter == null) throw new KavitaException(await _localizationService.TranslateAsync(userId, "chapter-doesnt-exist"));
        }

        var user = await _unitOfWork.UserRepository.GetUserByIdAsync(userId, AppUserIncludes.UserPreferences, ct);
        if (user == null) throw new KavitaException(await _localizationService.TranslateAsync(userId, "user-doesnt-exist"));

        return (user, series, chapter);
    }

    #region Review

    public Task ScrobbleSeriesReviewUpdate(int userId, int seriesId, string? reviewTitle, string reviewBody, CancellationToken ct = default)
    {
        return ScrobbleReviewUpdate(null, userId, seriesId, null, reviewTitle, reviewBody, ct);
    }

    public Task ScrobbleChapterReviewUpdate(int userId, int seriesId, int chapterId, string? reviewTitle, string reviewBody, CancellationToken ct = default)
    {
        return ScrobbleReviewUpdate(null, userId, seriesId, chapterId, reviewTitle, reviewBody, ct);
    }

    private async Task ScrobbleReviewUpdate(List<ScrobbleProvider>? scrobbleProviders, int userId, int seriesId, int? chapterId, string? reviewTitle, string reviewBody, CancellationToken ct = default)
    {
        if (!await _licenseService.HasActiveLicense(ct: ct)) return;

        var (user, series, chapter) = await LoadScrobbleEventData(userId, seriesId, chapterId, ct);

        var providers = GetProvidersForScrobbleEvent(scrobbleProviders, ScrobbleEventType.Review, user, series);
        if (providers.Count == 0) return;

        _logger.LogInformation("Processing Scrobbling review event for {AppUserId} on {SeriesName}", userId, series.Name);

        foreach (var provider in providers)
        {
            if (await CheckIfCannotScrobble(provider, userId, seriesId, series)) continue;

            var scrobbleProviderService = _serviceProvider.GetRequiredKeyedService<IScrobbleProviderService>(provider);

            try
            {
                await scrobbleProviderService.ScrobbleReviewUpdate(user, series, chapter, reviewTitle, reviewBody, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error happened while trying to scrobble an review event for {UserId} - {Provider}", user.Id, provider);
            }
        }
    }

    #endregion

    #region Rating

    public Task ScrobbleSeriesRatingUpdate(int userId, int seriesId, float rating, CancellationToken ct = default)
    {
        return ScrobbleRatingUpdate(null, userId, seriesId, null, rating, ct);
    }

    public Task ScrobbleChapterRatingUpdate(int userId, int seriesId, int chapterId, float rating, CancellationToken ct = default)
    {
        return ScrobbleRatingUpdate(null, userId, seriesId, chapterId, rating, ct);
    }

    private async Task ScrobbleRatingUpdate(List<ScrobbleProvider>? scrobbleProviders, int userId, int seriesId, int? chapterId, float rating, CancellationToken ct = default)
    {
        if (!await _licenseService.HasActiveLicense(ct: ct)) return;

        var (user, series, chapter) = await LoadScrobbleEventData(userId, seriesId, chapterId, ct);

        var providers = GetProvidersForScrobbleEvent(scrobbleProviders, ScrobbleEventType.ScoreUpdated, user, series);
        if (providers.Count == 0) return;

        _logger.LogInformation("Processing Scrobbling rating event for {AppUserId} on {SeriesName}", userId, series.Name);

        foreach (var provider in providers)
        {
            if (await CheckIfCannotScrobble(provider, userId, seriesId, series)) continue;

            var scrobbleProviderService = _serviceProvider.GetRequiredKeyedService<IScrobbleProviderService>(provider);

            try
            {
                await scrobbleProviderService.ScrobbleRatingUpdate(user, series, chapter, rating, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error happened while trying to scrobble an rating event for {UserId} - {Provider}", user.Id, provider);
            }
        }
    }

    #endregion

    #region Reading

    public async Task ScrobbleReadingUpdate(int userId, int seriesId, int chapterId, CancellationToken ct = default)
    {
        if (!await _licenseService.HasActiveLicense(ct: ct)) return;

        var (user, series, chapter) = await LoadScrobbleEventData(userId, seriesId, chapterId, ct);

        var providers = GetProvidersForScrobbleEvent(null, ScrobbleEventType.ChapterRead, user, series);
        if (providers.Count == 0) return;

        _logger.LogInformation("Processing Scrobbling reading event for {AppUserId} on {SeriesName}", userId, series.Name);

        foreach (var provider in providers)
        {
            if (await CheckIfCannotScrobble(provider, userId, seriesId, series)) continue;

            var scrobbleProviderService = _serviceProvider.GetRequiredKeyedService<IScrobbleProviderService>(provider);

            try
            {
                // We know chapter isn't null because chapterId isn't null
                await scrobbleProviderService.ScrobbleReadingUpdate(user, series, chapter!, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error happened while trying to scrobble an reading event for {UserId} - {Provider}", user.Id, provider);
            }
        }
    }

    public Task ScrobbleReadingUpdateForSeries(int userId, int seriesId, CancellationToken ct = default)
    {
        return ScrobbleReadingUpdateForSeries(null, userId, seriesId, ct);
    }

    private async Task ScrobbleReadingUpdateForSeries(List<ScrobbleProvider>? providers, int userId, int seriesId, CancellationToken ct = default)
    {
        if (!await _licenseService.HasActiveLicense(ct: ct)) return;

        var user = await _unitOfWork.UserRepository.GetUserByIdAsync(userId, ct: ct);
        if (user == null) throw new KavitaException(await _localizationService.TranslateAsync(userId, "user-doesnt-exist"));

        var series = await _unitOfWork.SeriesRepository.GetSeriesByIdAsync(seriesId, ScrobbleSeriesIncludes | SeriesIncludes.Chapters, ct);
        if (series == null) throw new KavitaException(await _localizationService.TranslateAsync(userId, "series-doesnt-exist"));

        var allChapters = series.Volumes.SelectMany(v => v.Chapters).ToList();

        await ScrobbleReadingUpdatesForChaptersSmart(providers, user, series, allChapters, ct);
    }

    public async Task ScrobbleReadingUpdateForVolume(int userId, int volumeId, CancellationToken ct = default)
    {
        if (!await _licenseService.HasActiveLicense(ct: ct)) return;

        var user = await _unitOfWork.UserRepository.GetUserByIdAsync(userId, ct: ct);
        if (user == null) throw new KavitaException(await _localizationService.TranslateAsync(userId, "user-doesnt-exist"));

        var volume = await _unitOfWork.VolumeRepository.GetVolumeByIdAsync(volumeId, VolumeIncludes.Chapters, ct: ct);
        if (volume == null) throw new KavitaException(await _localizationService.TranslateAsync(userId, "volume-doesnt-exist"));

        var series = await _unitOfWork.SeriesRepository.GetSeriesByIdAsync(volume.SeriesId, ScrobbleSeriesIncludes, ct);
        if (series == null) throw new KavitaException(await _localizationService.TranslateAsync(userId, "series-doesnt-exist"));

        var allChapters = volume.Chapters.ToList();

        await ScrobbleReadingUpdatesForChaptersSmart(null, user, series, allChapters, ct);
    }

    public async Task ScrobbleReadingUpdateForChapters(int userId, int seriesId, List<int> chapterIds, CancellationToken ct = default)
    {
        if (chapterIds.Count == 0) return;

        if (!await _licenseService.HasActiveLicense(ct: ct)) return;

        var user = await _unitOfWork.UserRepository.GetUserByIdAsync(userId, ct: ct);
        if (user == null) throw new KavitaException(await _localizationService.TranslateAsync(userId, "user-doesnt-exist"));

        var series = await _unitOfWork.SeriesRepository.GetSeriesByIdAsync(seriesId, ScrobbleSeriesIncludes, ct);
        if (series == null) throw new KavitaException(await _localizationService.TranslateAsync(userId, "series-doesnt-exist"));

        var chapters = await _unitOfWork.ChapterRepository.GetChaptersByIdsAsync(chapterIds, ct: ct);

        await ScrobbleReadingUpdatesForChaptersSmart(null, user, series, chapters.ToList(), ct);
    }

    private async Task ScrobbleReadingUpdatesForChaptersSmart(List<ScrobbleProvider>? scrobbleProviders, AppUser user, Series series, List<Chapter> chapters, CancellationToken ct = default)
    {
        var providers = GetProvidersForScrobbleEvent(scrobbleProviders, ScrobbleEventType.ChapterRead, user, series);
        if (providers.Count == 0) return;

        _logger.LogInformation("Processing Scrobbling reading event for {AppUserId} on {SeriesName} for {Count} chapters",
            user.Id, series.Name, chapters.Count);

        foreach (var provider in providers)
        {
            var scrobbleProviderService = _serviceProvider.GetRequiredKeyedService<IScrobbleProviderService>(provider);

            // Explicit check to reduce DB calls and work done
            if (scrobbleProviderService is ISeriesScrobbleService)
            {
                var firstChapter = chapters.FirstOrDefault();
                if (firstChapter != null)
                {
                    await scrobbleProviderService.ScrobbleReadingUpdate(user, series, firstChapter, ct);
                }
            }
            else
            {
                foreach (var chapter in chapters)
                {
                    await scrobbleProviderService.ScrobbleReadingUpdate(user, series, chapter, ct);
                }
            }
        }
    }

    #endregion

    public Task ScrobbleWantToReadUpdate(int userId, int seriesId, bool onWantToRead,
        CancellationToken ct = default)
    {
        return ScrobbleWantToReadUpdate(null, userId, seriesId, onWantToRead, ct);
    }

    private async Task ScrobbleWantToReadUpdate(List<ScrobbleProvider>? scrobbleProviders, int userId, int seriesId, bool onWantToRead, CancellationToken ct = default)
    {
        if (!await _licenseService.HasActiveLicense(ct: ct)) return;

        var user = await _unitOfWork.UserRepository.GetUserByIdAsync(userId, ct: ct);
        if (user == null) throw new KavitaException(await _localizationService.TranslateAsync(userId, "user-doesnt-exist"));

        var series = await _unitOfWork.SeriesRepository.GetSeriesByIdAsync(seriesId, ScrobbleSeriesIncludes | SeriesIncludes.Chapters, ct);
        if (series == null) throw new KavitaException(await _localizationService.TranslateAsync(userId, "series-doesnt-exist"));

        var eventType = onWantToRead ? ScrobbleEventType.AddWantToRead : ScrobbleEventType.RemoveWantToRead;

        var providers = GetProvidersForScrobbleEvent(scrobbleProviders, eventType, user, series);
        if (providers.Count == 0) return;

        _logger.LogInformation("Processing Scrobbling reading event for {AppUserId} on {SeriesName}", userId, series.Name);

        var allChapters = series.Volumes.SelectMany(v => v.Chapters).ToList();
        if (allChapters.Count == 0) return;

        var firstChapter = allChapters[0];

        foreach (var provider in providers)
        {
            if (await CheckIfCannotScrobble(provider, userId, seriesId, series)) continue;

            var scrobbleProviderService = _serviceProvider.GetRequiredKeyedService<IScrobbleProviderService>(provider);

            try
            {
                // As WantToRead updates are always at a series level, we split here to avoid DB calls
                if (scrobbleProviderService is ISeriesScrobbleService)
                {
                    await scrobbleProviderService.ScrobbleWantToReadUpdate(user, series, firstChapter, onWantToRead, ct);
                }
                else
                {
                    foreach (var chapter in allChapters)
                    {
                        await scrobbleProviderService.ScrobbleWantToReadUpdate(user, series, chapter, onWantToRead, ct);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error happened while trying to scrobble a want to read event for {UserId} - {Provider}", user.Id, provider);
            }
        }
    }

    #endregion

    /// <summary>
    /// Returns false if the series cannot be scrobbled for the given provider. I.e. not matched for that provider
    /// series on hold, or the library is not eligible
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="seriesId"></param>
    /// <param name="series">Should have Library resolved</param>
    /// <returns></returns>
    private async Task<bool> CheckIfCannotScrobble(ScrobbleProvider provider, int userId, int seriesId, Series series)
    {
        // TODO: This needs updating to take the provider into account (Dict?)
        if (series.DontMatch) return true;

        if (await _unitOfWork.UserRepository.HasHoldOnSeries(userId, seriesId))
        {
            _logger.LogInformation("Series {SeriesName} is on AppUserId {AppUserId}'s hold list. Not scrobbling", series.Name, userId);
            return true;
        }

        var library = series.Library ?? await _unitOfWork.LibraryRepository.GetLibraryForIdAsync(series.LibraryId);
        return library == null || !ExternalMetadataService.IsPlusEligible(library.Type);
    }

    /// <summary>
    /// Returns the rate limit from the K+ api
    /// </summary>
    /// <param name="license"></param>
    /// <param name="aniListToken"></param>
    /// <returns></returns>
    private async Task<int> GetRateLimit(string license, string aniListToken)
    {
        if (string.IsNullOrWhiteSpace(aniListToken)) return 0;

        try
        {
            return await _kavitaPlusApiService.GetRateLimitAsync(license, aniListToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error happened trying to get rate limit from Kavita+ API");
        }

        return 0;
    }

    #region Scrobble process (Requests to K+)

    /// <summary>
    /// Retrieve all events for which the series has not errored, then delete all current errors
    /// </summary>
    private async Task<ScrobbleSyncContext> PrepareScrobbleContext()
    {
        var librariesWithScrobbling = (await _unitOfWork.LibraryRepository.GetLibrariesAsync())
            .AsEnumerable()
            .Select(l => l.Id)
            .ToImmutableHashSet();

        var erroredSeries = (await _unitOfWork.ScrobbleRepository.GetScrobbleErrors())
            .Where(e => e.Comment is "Unknown Series" or UnknownSeriesErrorMessage or AccessTokenErrorMessage)
            .Select(e => e.SeriesId)
            .ToList();

        var readEvents = (await _unitOfWork.ScrobbleRepository.GetByEvent(ScrobbleEventType.ChapterRead))
            .Where(e => librariesWithScrobbling.Contains(e.LibraryId))
            .Where(e => !erroredSeries.Contains(e.SeriesId))
            .ToList();
        var addToWantToRead = (await _unitOfWork.ScrobbleRepository.GetByEvent(ScrobbleEventType.AddWantToRead))
            .Where(e => librariesWithScrobbling.Contains(e.LibraryId))
            .Where(e => !erroredSeries.Contains(e.SeriesId))
            .ToList();
        var removeWantToRead = (await _unitOfWork.ScrobbleRepository.GetByEvent(ScrobbleEventType.RemoveWantToRead))
            .Where(e => librariesWithScrobbling.Contains(e.LibraryId))
            .Where(e => !erroredSeries.Contains(e.SeriesId))
            .ToList();
        var ratingEvents = (await _unitOfWork.ScrobbleRepository.GetByEvent(ScrobbleEventType.ScoreUpdated))
            .Where(e => librariesWithScrobbling.Contains(e.LibraryId))
            .Where(e => !erroredSeries.Contains(e.SeriesId))
            .ToList();

        return new ScrobbleSyncContext
        {
            ReadEvents = readEvents,
            RatingEvents = ratingEvents,
            AddToWantToRead = addToWantToRead,
            RemoveWantToRead = removeWantToRead,
            Decisions = CalculateNetWantToReadDecisions(addToWantToRead, removeWantToRead),
            RateLimits = [],
            License = (await _unitOfWork.SettingsRepository.GetSettingAsync(ServerSettingKey.LicenseKey)).Value,
        };
    }

    /// <summary>
    /// Filters users who can scrobble, sets their rate limit and updates the <see cref="ScrobbleSyncContext.Users"/>
    /// </summary>
    /// <param name="ctx"></param>
    /// <returns></returns>
    private async Task PrepareUsersToScrobble(ScrobbleSyncContext ctx)
    {
        // For all userIds, ensure that we can connect and have access
        var usersToScrobble = ctx.ReadEvents.Select(r => r.AppUser)
            .Concat(ctx.AddToWantToRead.Select(r => r.AppUser))
            .Concat(ctx.RemoveWantToRead.Select(r => r.AppUser))
            .Concat(ctx.RatingEvents.Select(r => r.AppUser))
            .Where(user => !string.IsNullOrEmpty(user.AniListAccessToken))
            .Where(user => user.UserPreferences.AniListScrobblingEnabled)
            .DistinctBy(u => u.Id)
            .ToList();

        foreach (var user in usersToScrobble)
        {
            await SetAndCheckRateLimit(ctx.RateLimits, user, ctx.License);
        }

        ctx.Users = usersToScrobble;
    }

    /// <summary>
    /// Cleans up any events that are due to bugs or legacy
    /// </summary>
    private async Task CleanupOldOrBuggedEvents()
    {
        try
        {
            var eventsWithoutAnilistToken = (await _unitOfWork.ScrobbleRepository.GetEvents())
                .Where(e => e is { IsProcessed: false, IsErrored: false })
                .Where(e => string.IsNullOrEmpty(e.AppUser.AniListAccessToken));

            _unitOfWork.ScrobbleRepository.Remove(eventsWithoutAnilistToken);
            await _unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "There was an exception when trying to delete old scrobble events when the user has no active token");
        }
    }

    /// <summary>
    /// This is a task that is run on a fixed schedule (every few hours or every day) that clears out the scrobble event table
    /// and offloads the data to the API server which performs the syncing to the providers.
    /// </summary>
    /// <param name="ct"></param>
    [DisableConcurrentExecution(60 * 60 * 60)]
    [AutomaticRetry(Attempts = 3, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
    public async Task ProcessUpdatesSinceLastSync(CancellationToken ct = default)
    {
        var ctx = await PrepareScrobbleContext();
        if (ctx.TotalCount == 0) return;

        // Get all the applicable users to scrobble and set their rate limits
        await PrepareUsersToScrobble(ctx);

        _logger.LogInformation("Scrobble Processing Details:" +
                               "\n  Read Events: {ReadEventsCount}" +
                               "\n  Want to Read Events: {WantToReadEventsCount}" +
                               "\n  Rating Events: {RatingEventsCount}" +
                               "\n  Users to Scrobble: {UsersToScrobbleCount}"  +
                               "\n  Total Events to Process: {TotalEvents}",
            ctx.ReadEvents.Count,
            ctx.Decisions.Count,
            ctx.RatingEvents.Count,
            ctx.Users.Count,
            ctx.TotalCount);

        try
        {
            await ProcessReadEvents(ctx);
            await ProcessRatingEvents(ctx);
            await ProcessWantToReadRatingEvents(ctx);
        }
        catch (FlurlHttpException ex)
        {
            _logger.LogError(ex, "Kavita+ API or a Scrobble service may be experiencing an outage. Stopping sending data");
            return;
        }


        await SaveToDb(ctx.ProgressCounter, true);
        _logger.LogInformation("Scrobbling Events is complete");

        await CleanupOldOrBuggedEvents();
    }

    /// <summary>
    /// Calculates the net want-to-read decisions by considering all events.
    /// Returns events that represent the final state for each user/series pair.
    /// </summary>
    /// <param name="addEvents">List of events for adding to want-to-read</param>
    /// <param name="removeEvents">List of events for removing from want-to-read</param>
    /// <returns>List of events that represent the final state (add or remove)</returns>
    private static List<ScrobbleEvent> CalculateNetWantToReadDecisions(List<ScrobbleEvent> addEvents, List<ScrobbleEvent> removeEvents)
    {
        // Create a dictionary to track the latest event for each user/series combination
        var latestEvents = new Dictionary<(int SeriesId, int AppUserId), ScrobbleEvent>();

        // Process all add events
        foreach (var addEvent in addEvents)
        {
            var key = (addEvent.SeriesId, addEvent.AppUserId);

            if (latestEvents.TryGetValue(key, out var value) && addEvent.CreatedUtc <= value.CreatedUtc) continue;

            value = addEvent;
            latestEvents[key] = value;
        }

        // Process all remove events
        foreach (var removeEvent in removeEvents)
        {
            var key = (removeEvent.SeriesId, removeEvent.AppUserId);

            if (latestEvents.TryGetValue(key, out var value) && removeEvent.CreatedUtc <= value.CreatedUtc) continue;

            value = removeEvent;
            latestEvents[key] = value;
        }

        // Return all events that represent the final state
        return latestEvents.Values.ToList();
    }

    private async Task ProcessWantToReadRatingEvents(ScrobbleSyncContext ctx)
    {
        await ProcessEvents(ctx.Decisions, ctx, evt => Task.FromResult(new ScrobbleDto
            {
                Format = evt.Format,
                AniListId = evt.AniListId,
                MALId = (int?) evt.MalId,
                ScrobbleEventType = evt.ScrobbleEventType,
                ChapterNumber = evt.ChapterNumber,
                VolumeNumber = (int?) evt.VolumeNumber,
                AniListToken = evt.AppUser.AniListAccessToken ?? string.Empty,
                SeriesName = evt.Series.Name,
                LocalizedSeriesName = evt.Series.LocalizedName,
                Year = evt.Series.Metadata.ReleaseYear
            }));

        // After decisions, we need to mark all the want to read and remove from want to read as completed
        var processedDecisions = ctx.Decisions.Where(d => d.IsProcessed).ToList();
        if (processedDecisions.Count > 0)
        {
            foreach (var scrobbleEvent in processedDecisions)
            {
                scrobbleEvent.IsProcessed = true;
                scrobbleEvent.ProcessDateUtc = DateTime.UtcNow;
                _unitOfWork.ScrobbleRepository.Update(scrobbleEvent);
            }
            await _unitOfWork.CommitAsync();
        }
    }

    private async Task ProcessRatingEvents(ScrobbleSyncContext ctx)
    {
        await ProcessEvents(ctx.RatingEvents, ctx, evt => Task.FromResult(new ScrobbleDto
            {
                Format = evt.Format,
                AniListId = evt.AniListId,
                MALId = (int?) evt.MalId,
                ScrobbleEventType = evt.ScrobbleEventType,
                AniListToken = evt.AppUser.AniListAccessToken ?? string.Empty,
                SeriesName = evt.Series.Name,
                LocalizedSeriesName = evt.Series.LocalizedName,
                Rating = evt.Rating,
                Year = evt.Series.Metadata.ReleaseYear
            }));
    }

    private async Task ProcessReadEvents(ScrobbleSyncContext ctx)
    {
        // Recalculate the highest volume/chapter
        foreach (var readEvt in ctx.ReadEvents)
        {
            // Note: this causes skewing in the scrobble history because it makes it look like there are duplicate events
            readEvt.VolumeNumber =
                (int) await _unitOfWork.AppUserProgressRepository.GetHighestFullyReadVolumeForSeries(readEvt.SeriesId,
                    readEvt.AppUser.Id);
            readEvt.ChapterNumber =
                await _unitOfWork.AppUserProgressRepository.GetHighestFullyReadChapterForSeries(readEvt.SeriesId,
                    readEvt.AppUser.Id);
            _unitOfWork.ScrobbleRepository.Update(readEvt);
        }

        await ProcessEvents(ctx.ReadEvents, ctx, async evt => new ScrobbleDto
            {
                Format = evt.Format,
                AniListId = evt.AniListId,
                MALId = (int?) evt.MalId,
                ScrobbleEventType = evt.ScrobbleEventType,
                ChapterNumber = evt.ChapterNumber,
                VolumeNumber = (int?) evt.VolumeNumber,
                AniListToken = evt.AppUser.AniListAccessToken  ?? string.Empty,
                SeriesName = evt.Series.Name,
                LocalizedSeriesName = evt.Series.LocalizedName,
                ScrobbleDateUtc = evt.LastModifiedUtc,
                Year = evt.Series.Metadata.ReleaseYear,
                StartedReadingDateUtc = await _unitOfWork.AppUserProgressRepository.GetFirstProgressForSeries(evt.SeriesId, evt.AppUser.Id),
                LatestReadingDateUtc = await _unitOfWork.AppUserProgressRepository.GetLatestProgressForSeries(evt.SeriesId, evt.AppUser.Id),
            });
    }

    /// <summary>
    /// Returns true if the user token is valid
    /// </summary>
    /// <param name="evt"></param>
    /// <returns></returns>
    /// <remarks>If the token is not, adds a scrobble error</remarks>
    private async Task<bool> ValidateUserToken(ScrobbleEvent evt)
    {
        if (!TokenService.HasTokenExpired(evt.AppUser.AniListAccessToken))
            return true;

        _unitOfWork.ScrobbleRepository.Attach(new ScrobbleError
        {
            Comment = "AniList token has expired and needs rotating. Scrobbling wont work until then",
            Details = $"User: {evt.AppUser.UserName}, Expired: {TokenService.GetTokenExpiry(evt.AppUser.AniListAccessToken)}",
            LibraryId = evt.LibraryId,
            SeriesId = evt.SeriesId
        });
        await _unitOfWork.CommitAsync();
        return false;
    }

    /// <summary>
    /// Returns true if the series can be scrobbled
    /// </summary>
    /// <param name="evt"></param>
    /// <returns></returns>
    /// <remarks>If the series cannot be scrobbled, adds a scrobble error</remarks>
    private async Task<bool> ValidateSeriesCanBeScrobbled(ScrobbleEvent evt)
    {
        if (evt.Series is { IsBlacklisted: false, DontMatch: false })
            return true;

        _logger.LogInformation("Series {SeriesName} ({SeriesId}) can't be matched and thus cannot scrobble this event",
            evt.Series.Name, evt.SeriesId);

        _unitOfWork.ScrobbleRepository.Attach(new ScrobbleError
        {
            Comment = UnknownSeriesErrorMessage,
            Details = $"User: {evt.AppUser.UserName} Series: {evt.Series.Name}",
            LibraryId = evt.LibraryId,
            SeriesId = evt.SeriesId
        });

        evt.SetErrorMessage(UnknownSeriesErrorMessage);
        evt.ProcessDateUtc = DateTime.UtcNow;
        _unitOfWork.ScrobbleRepository.Update(evt);
        await _unitOfWork.CommitAsync();
        return false;
    }

    /// <summary>
    /// Removed Special parses numbers from chatter and volume numbers
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private static ScrobbleDto NormalizeScrobbleData(ScrobbleDto data)
    {
        // We need to handle the encoding and changing it to the old one until we can update the API layer to handle these
        // which could happen in v0.8.3
        if (data.VolumeNumber is Parser.SpecialVolumeNumber or Parser.DefaultChapterNumber)
        {
            data.VolumeNumber = 0;
        }


        if (data.ChapterNumber is Parser.DefaultChapterNumber)
        {
            data.ChapterNumber = 0;
        }


        return data;
    }

    /// <summary>
    /// Loops through all events, and post them to K+
    /// </summary>
    /// <param name="events"></param>
    /// <param name="ctx"></param>
    /// <param name="createEvent"></param>
    private async Task ProcessEvents(IEnumerable<ScrobbleEvent> events, ScrobbleSyncContext ctx, Func<ScrobbleEvent, Task<ScrobbleDto>> createEvent)
    {
        foreach (var evt in events.Where(CanProcessScrobbleEvent))
        {
            _logger.LogDebug("Processing Scrobble Events: {Count} / {Total}", ctx.ProgressCounter, ctx.TotalCount);
            ctx.ProgressCounter++;

            if (!await ValidateUserToken(evt)) continue;
            if (!await ValidateSeriesCanBeScrobbled(evt)) continue;

            var count = await SetAndCheckRateLimit(ctx.RateLimits, evt.AppUser, ctx.License);
            if (count == 0)
            {
                if (ctx.Users.Count == 1) break;
                continue;
            }

            try
            {
                var data = NormalizeScrobbleData(await createEvent(evt));

                ctx.RateLimits[evt.AppUserId] = await PostScrobbleUpdate(data, ctx.License, evt);

                evt.IsProcessed = true;
                evt.ProcessDateUtc = DateTime.UtcNow;
                _unitOfWork.ScrobbleRepository.Update(evt);
            }
            catch (FlurlHttpException)
            {
                // If a flurl exception occured, the API is likely down. Kill processing
                throw;
            }
            catch (KavitaException ex)
            {
                if (ex.Message.Contains("Access token is invalid"))
                {
                    _logger.LogCritical(ex, "Access Token for AppUserId: {AppUserId} needs to be regenerated/renewed to continue scrobbling", evt.AppUser.Id);
                    evt.SetErrorMessage(AccessTokenErrorMessage);
                    _unitOfWork.ScrobbleRepository.Update(evt);

                    // Ensure series with this error do not get re-processed next sync
                    _unitOfWork.ScrobbleRepository.Attach(new ScrobbleError
                    {
                        Comment = AccessTokenErrorMessage,
                        Details = $"{evt.AppUser.UserName} has an invalid access token (K+ Error)",
                        LibraryId = evt.LibraryId,
                        SeriesId = evt.SeriesId,
                    });
                }
            }
            catch (Exception ex)
            {
                /* Swallow as it's already been handled in PostScrobbleUpdate */
                _logger.LogError(ex, "Error processing event {EventId}", evt.Id);
            }

            await SaveToDb(ctx.ProgressCounter);

            // We can use count to determine how long to sleep based on rate gain. It might be specific to AniList, but we can model others
            var delay = count > 10 ? TimeSpan.FromMilliseconds(ScrobbleSleepTime) : TimeSpan.FromSeconds(60);
            await Task.Delay(delay);
        }

        await SaveToDb(ctx.ProgressCounter, true);
    }

    /// <summary>
    /// Save changes every five updates
    /// </summary>
    /// <param name="progressCounter"></param>
    /// <param name="force">Ignore update count check</param>
    private async Task SaveToDb(int progressCounter, bool force = false)
    {
        if ((force || progressCounter % 5 == 0) && _unitOfWork.HasChanges())
        {
            _logger.LogDebug("Saving Scrobbling Event Processing Progress");
            await _unitOfWork.CommitAsync();
        }
    }

    /// <summary>
    /// If no errors have been logged for the given series, creates a new Unknown series error, and blacklists the series
    /// </summary>
    /// <param name="data"></param>
    /// <param name="evt"></param>
    private async Task MarkSeriesAsUnknown(ScrobbleDto data, ScrobbleEvent evt)
    {
        if (await _unitOfWork.ScrobbleRepository.HasErrorForSeries(evt.SeriesId)) return;

        // Create a new ExternalMetadata entry to indicate that this is not matchable
        var series = await _unitOfWork.SeriesRepository.GetSeriesByIdAsync(evt.SeriesId, SeriesIncludes.ExternalMetadata);
        if (series == null) return;

        series.ExternalSeriesMetadata ??= new ExternalSeriesMetadata {SeriesId = evt.SeriesId};
        series.IsBlacklisted = true;
        _unitOfWork.SeriesRepository.Update(series);

        _unitOfWork.ScrobbleRepository.Attach(new ScrobbleError
        {
            Comment = UnknownSeriesErrorMessage,
            Details = data.SeriesName,
            LibraryId = evt.LibraryId,
            SeriesId = evt.SeriesId
        });
    }

    /// <summary>
    /// Makes the K+ request, and handles any exceptions that occur
    /// </summary>
    /// <param name="data">Data to send to K+</param>
    /// <param name="license">K+ license key</param>
    /// <param name="evt">Related scrobble event</param>
    /// <returns></returns>
    /// <exception cref="KavitaException">Exceptions may be rethrown as a KavitaException</exception>
    /// <remarks>Some FlurlHttpException are also rethrown</remarks>
    public async Task<int> PostScrobbleUpdate(ScrobbleDto data, string license, ScrobbleEvent evt)
    {
        try
        {
            var response = await _kavitaPlusApiService.PostScrobbleUpdateAsync(data, license);

            _logger.LogDebug("K+ API Scrobble response for series {SeriesName}: Successful {Successful}, ErrorMessage {ErrorMessage}, ExtraInformation: {ExtraInformation}, RateLeft: {RateLeft}",
                data.SeriesName, response.Successful, response.ErrorMessage, response.ExtraInformation, response.RateLeft);

            if (response.Successful || response.ErrorMessage == null) return response.RateLeft;

            // Might want to log this under ScrobbleError
            if (response.ErrorMessage.Contains("Too Many Requests"))
            {
                _logger.LogInformation("Hit Too many requests while posting scrobble updates, sleeping to regain requests and retrying");
                await Task.Delay(TimeSpan.FromMinutes(10));
                return await PostScrobbleUpdate(data, license, evt);
            }

            if (response.ErrorMessage.Contains("Unauthorized"))
            {
                _logger.LogCritical("Kavita+ responded with Unauthorized. Please check your subscription");
                await _licenseService.HasActiveLicense(true);
                evt.SetErrorMessage(InvalidKPlusLicenseErrorMessage);
                throw new KavitaException("Kavita+ responded with Unauthorized. Please check your subscription");
            }

            if (response.ErrorMessage.Contains("Access token is invalid"))
            {
                evt.SetErrorMessage(AccessTokenErrorMessage);
                throw new KavitaException("Access token is invalid");
            }

            if (response.ErrorMessage.Contains("Unknown Series"))
            {
                // Log the Series name and Id in ScrobbleErrors
                _logger.LogInformation("Kavita+ was unable to match the series: {SeriesName}", evt.Series.Name);
                await MarkSeriesAsUnknown(data, evt);
                evt.SetErrorMessage(UnknownSeriesErrorMessage);
            } else if (response.ErrorMessage.StartsWith("Review"))
            {
                // Log the Series name and Id in ScrobbleErrors
                _logger.LogInformation("Kavita+ was unable to save the review");
                if (!await _unitOfWork.ScrobbleRepository.HasErrorForSeries(evt.SeriesId))
                {
                    _unitOfWork.ScrobbleRepository.Attach(new ScrobbleError()
                    {
                        Comment = response.ErrorMessage,
                        Details = data.SeriesName,
                        LibraryId = evt.LibraryId,
                        SeriesId = evt.SeriesId
                    });
                }
                evt.SetErrorMessage(ReviewFailedErrorMessage);
            }

            return response.RateLeft;
        }
        catch (FlurlHttpException ex)
        {
            var errorMessage = await ex.GetResponseStringAsync();
            // Trim quotes if the response is a JSON string
            errorMessage = errorMessage.Trim('"');

            if (errorMessage.Contains("Too Many Requests"))
            {
                _logger.LogInformation("Hit Too many requests while posting scrobble updates, sleeping to regain requests and retrying");
                await Task.Delay(TimeSpan.FromMinutes(10));
                return await PostScrobbleUpdate(data, license, evt);
            }

            _logger.LogError(ex, "Scrobbling to Kavita+ API failed due to error: {ErrorMessage}", ex.Message);
            if (ex.StatusCode == 500 || ex.Message.Contains("Call failed with status code 500 (Internal Server Error)"))
            {
                if (!await _unitOfWork.ScrobbleRepository.HasErrorForSeries(evt.SeriesId))
                {
                    _unitOfWork.ScrobbleRepository.Attach(new ScrobbleError()
                    {
                        Comment = UnknownSeriesErrorMessage,
                        Details = data.SeriesName,
                        LibraryId = evt.LibraryId,
                        SeriesId = evt.SeriesId
                    });
                }
                evt.SetErrorMessage(BadPayLoadErrorMessage);
                throw new KavitaException(BadPayLoadErrorMessage);
            }
            throw;
        }
    }

    #endregion

    #region BackFill

    /// <summary>
    /// This will backfill events from existing progress history, ratings, and want to read for users that have a valid license
    /// </summary>
    /// <param name="scrobbleProvider"></param>
    /// <param name="userId">Defaults to 0 meaning all users. Allows a userId to be set if a scrobble key is added to a user</param>
    /// <param name="ct"></param>
    public async Task CreateEventsFromExistingHistory(ScrobbleProvider scrobbleProvider, int userId = 0,
        CancellationToken ct = default)
    {
        if (!await _licenseService.HasActiveLicense(ct: ct)) return;

        if (userId != 0)
        {
            var user = await _unitOfWork.UserRepository.GetUserByIdAsync(userId, ct: ct);
            if (user == null) return;
            if (!user.ScrobbleProviders.TryGetValue(scrobbleProvider, out var scrobbleProviderSettings)
                || scrobbleProviderSettings.HasRunScrobbleEventGeneration
                || string.IsNullOrEmpty(scrobbleProviderSettings.AuthenticationToken))
            {
                _logger.LogWarning("User {UserName} has already run scrobble event generation for {Provider}, Kavita will not generate more events", user.UserName, scrobbleProvider);
                return;
            }
        }

        var userIds = (await _unitOfWork.UserRepository.GetAllUsersAsync(ct: ct))
            .Where(l => userId == 0 || userId == l.Id)
            .Where(u => u.ScrobbleProviders.TryGetValue(scrobbleProvider, out var scrobbleProviderSettings)
                        && !scrobbleProviderSettings.HasRunScrobbleEventGeneration
                        && !string.IsNullOrEmpty(scrobbleProviderSettings.AuthenticationToken))
            .Select(u => u.Id);

        foreach (var uId in userIds)
        {
            await CreateEventsFromExistingHistoryForUser(scrobbleProvider, uId, ct);
        }
    }

    /// <summary>
    /// Creates wantToRead, rating, reviews, and series progress events for the suer
    /// </summary>
    /// <param name="scrobbleProvider"></param>
    /// <param name="userId"></param>
    /// <param name="ct"></param>
    private async Task CreateEventsFromExistingHistoryForUser(ScrobbleProvider scrobbleProvider, int userId, CancellationToken ct)
    {
        List<ScrobbleProvider> providers = [scrobbleProvider];

        var wantToRead = await _unitOfWork.SeriesRepository.GetWantToReadForUserAsync(userId, ct);
        foreach (var wtr in wantToRead)
        {
            await ScrobbleWantToReadUpdate(providers, userId, wtr.Id, true, ct);
        }

        var ratings = await _unitOfWork.UserRepository.GetSeriesWithRatings(userId, ct);
        foreach (var rating in ratings)
        {
            await ScrobbleRatingUpdate(providers, userId, rating.SeriesId, null, rating.Rating, ct);
        }

        var chapterRatings = await _unitOfWork.UserRepository.GetChaptersWithRatings(userId, ct);
        foreach (var chapterRating in chapterRatings)
        {
            await ScrobbleRatingUpdate(providers, userId, chapterRating.SeriesId, chapterRating.ChapterId, chapterRating.Rating, ct);
        }

        var reviews = await _unitOfWork.UserRepository.GetSeriesWithReviews(userId, ct);
        foreach (var review in reviews)
        {
            await ScrobbleReviewUpdate(providers, userId, review.SeriesId, null, string.Empty, review.Review!, ct);
        }

        var chapterReviews = await _unitOfWork.UserRepository.GetChaptersWithReviews(userId, ct);
        foreach (var chapterReview in chapterReviews)
        {
            await ScrobbleReviewUpdate(providers, userId, chapterReview.SeriesId, chapterReview.ChapterId, string.Empty,
                chapterReview.Review!, ct);
        }

        var filter = new SeriesFilterV2Dto
        {
            Combination = FilterCombination.And,
            Statements =
            [
                new SeriesFilterStatementDto
                {
                    Comparison = FilterComparison.LessThan,
                    Field = SeriesFilterField.ReadProgress,
                    Value = "100"
                },
                new SeriesFilterStatementDto
                {
                    Comparison = FilterComparison.GreaterThan,
                    Field = SeriesFilterField.ReadProgress,
                    Value = "0"
                }
            ]
        };

        var seriesWithProgress =
            await _unitOfWork.SeriesRepository.GetSeriesDtoForLibraryIdAsync(userId, new UserParams(), filter, ct: ct);

        foreach (var series in seriesWithProgress.Where(series => series.PagesRead > 0))
        {
            await ScrobbleReadingUpdateForSeries(providers, userId, series.Id, ct);
        }

        var user = await _unitOfWork.UserRepository.GetUserByIdAsync(userId, ct: ct);
        if (user != null)
        {
            if (user.ScrobbleProviders.TryGetValue(scrobbleProvider, out var scrobbleProviderSettings))
            {
                scrobbleProviderSettings.HasRunScrobbleEventGeneration = true;
                scrobbleProviderSettings.ScrobbleEventGenerationRan = DateTime.UtcNow;

                _unitOfWork.UserRepository.Update(user);
            }

            await _unitOfWork.CommitAsync(ct);
        }
    }

    public async Task CreateEventsFromExistingHistoryForSeries(int seriesId, CancellationToken ct = default)
    {
        if (!await _licenseService.HasActiveLicense(ct: ct)) return;

        var series = await _unitOfWork.SeriesRepository.GetSeriesByIdAsync(seriesId, SeriesIncludes.Library, ct);
        if (series == null) return;

        _logger.LogInformation("Creating Scrobbling events for Series {SeriesName}", series.Name);

        var userIds = (await _unitOfWork.UserRepository.GetAllUsersAsync(ct: ct)).Select(u => u.Id);

        foreach (var uId in userIds)
        {
            // Handle "Want to Read" updates specific to the series
            var wantToRead = await _unitOfWork.SeriesRepository.GetWantToReadForUserAsync(uId, ct);
            foreach (var wtr in wantToRead.Where(wtr => wtr.Id == seriesId))
            {
                await ScrobbleWantToReadUpdate(uId, wtr.Id, true, ct);
            }

            // Handle ratings specific to the series
            var ratings = await _unitOfWork.UserRepository.GetSeriesWithRatings(uId, ct);
            foreach (var rating in ratings.Where(rating => rating.SeriesId == seriesId))
            {
                await ScrobbleSeriesRatingUpdate(uId, rating.SeriesId, rating.Rating, ct);
            }

            var chapterRatings = await _unitOfWork.UserRepository.GetChaptersWithRatings(uId, ct);
            foreach (var chapterRating in chapterRatings.Where(chapterRating => chapterRating.SeriesId == seriesId))
            {
                await ScrobbleChapterRatingUpdate(uId, chapterRating.SeriesId, chapterRating.ChapterId, chapterRating.Rating, ct);
            }

            // Handle review specific to the series
            var reviews = await _unitOfWork.UserRepository.GetSeriesWithReviews(uId, ct);
            foreach (var review in reviews.Where(r => r.SeriesId == seriesId && !string.IsNullOrEmpty(r.Review)))
            {
                await ScrobbleSeriesReviewUpdate(uId, review.SeriesId, string.Empty, review.Review!, ct);
            }

            var chapterReviews = await _unitOfWork.UserRepository.GetChaptersWithReviews(uId, ct);
            foreach (var chapterReview in chapterReviews.Where(r =>
                         r.SeriesId == seriesId && !string.IsNullOrEmpty(r.Review)))
            {
                await ScrobbleChapterReviewUpdate(uId, chapterReview.SeriesId, chapterReview.ChapterId,
                    string.Empty, chapterReview.Review!, ct);
            }

            // Handle progress updates for the specific series
            await ScrobbleReadingUpdateForSeries(uId, seriesId, ct);
        }
    }

    #endregion

    /// <summary>
    /// Removes all events (active) that are tied to a now-on hold series
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="seriesId"></param>
    /// <param name="ct"></param>
    public async Task ClearEventsForSeries(int userId, int seriesId, CancellationToken ct = default)
    {
        _logger.LogInformation("Clearing Pre-existing Scrobble events for Series {SeriesId} by User {AppUserId} as Series is now on hold list", seriesId, userId);

        var events = await _unitOfWork.ScrobbleRepository.GetUserEventsForSeries(userId, seriesId, ct);
        _unitOfWork.ScrobbleRepository.Remove(events);
        await _unitOfWork.CommitAsync(ct);
    }

    public Task SyncProviderInfo(CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Removes all events that have been processed that are 7 days old
    /// </summary>
    /// <param name="ct"></param>
    [DisableConcurrentExecution(60 * 60 * 60)]
    [AutomaticRetry(Attempts = 3, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
    public async Task ClearProcessedEvents(CancellationToken ct = default)
    {
        const int daysAgo = 7;
        var events = await _unitOfWork.ScrobbleRepository.GetProcessedEvents(daysAgo, ct);
        _unitOfWork.ScrobbleRepository.Remove(events);
        _logger.LogInformation("Removing {Count} scrobble events that have been processed {DaysAgo}+ days ago", events.Count, daysAgo);
        await _unitOfWork.CommitAsync(ct);
    }

    private static bool CanProcessScrobbleEvent(ScrobbleEvent readEvent)
    {
        var userProviders = GetUserProviders(readEvent.AppUser);
        switch (readEvent.Series.Library.Type)
        {
            case LibraryType.Manga when MangaProviders.Intersect(userProviders).Any():
            case LibraryType.Comic when ComicProviders.Intersect(userProviders).Any():
            case LibraryType.Book when BookProviders.Intersect(userProviders).Any():
            case LibraryType.LightNovel when LightNovelProviders.Intersect(userProviders).Any():
                return true;
            default:
                return false;
        }
    }

    private static List<ScrobbleProvider> GetUserProviders(AppUser appUser)
    {
        var providers = new List<ScrobbleProvider>();
        if (!string.IsNullOrEmpty(appUser.AniListAccessToken)) providers.Add(ScrobbleProvider.AniList);

        return providers;
    }

    private async Task<int> SetAndCheckRateLimit(IDictionary<int, int> userRateLimits, AppUser user, string license)
    {
        if (string.IsNullOrEmpty(user.AniListAccessToken)) return 0;
        try
        {
            if (!userRateLimits.ContainsKey(user.Id))
            {
                var rate = await GetRateLimit(license, user.AniListAccessToken);
                userRateLimits.Add(user.Id, rate);
            }
        }
        catch (Exception ex)
        {
            _logger.LogInformation(ex, "User {UserName} had an issue figuring out rate: {Message}", user.UserName, ex.Message);
            userRateLimits.Add(user.Id, 0);
        }

        userRateLimits.TryGetValue(user.Id, out var count);
        if (count == 0)
        {
            _logger.LogInformation("User {UserName} is out of rate for Scrobbling", user.UserName);
        }

        return count;
    }

}
