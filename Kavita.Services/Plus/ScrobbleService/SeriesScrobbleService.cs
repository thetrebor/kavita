using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kavita.API.Database;
using Kavita.API.Services.Plus;
using Kavita.Models.DTOs.KavitaPlus;
using Kavita.Models.DTOs.Scrobbling;
using Kavita.Models.Entities;
using Kavita.Models.Entities.Enums;
using Kavita.Models.Entities.Enums.Audit;
using Kavita.Models.Entities.Enums.UserPreferences;
using Kavita.Models.Entities.Scrobble;
using Kavita.Models.Entities.User;
using Kavita.Models.Extensions;
using Kavita.Services.Scanner;
using Microsoft.Extensions.Logging;

namespace Kavita.Services.Plus.ScrobbleService;

public interface ISeriesScrobbleService : IScrobbleProviderService;

/// <summary>
/// A <see cref="IScrobbleProviderService"/> implementation for <see cref="ScrobbleProvider"/>'s that track data
/// based on series. (Mangabaka, AniList, etc.)
/// </summary>
public abstract class SeriesScrobbleService<T>(ILogger<T> logger, IUnitOfWork unitOfWork, IKavitaPlusAuditService auditService): ISeriesScrobbleService
where T: IScrobbleProviderService
{
    protected abstract ScrobbleProvider Provider { get; }

    protected abstract IReadOnlyList<ScrobbleEventType> SupportedEvents { get; }

    protected abstract void SetScrobbleIds(ScrobbleEvent evt, Series series);

    public abstract bool IsTokenValid(string token);

    public async Task ScrobbleReadStatusUpdates(AppUser user, Series series, Chapter? chapter, ScrobbleReadStatus status,
        CancellationToken ct = default)
    {
        if (!SupportedEvents.Contains(ScrobbleEventType.ReadStatusUpdate) || chapter != null) return;

        var existingEvent = await unitOfWork.ScrobbleRepository.GetEvent(
            Provider, user.Id, series.Id, null, ScrobbleEventType.ReadStatusUpdate, true, ct
        );

        if (existingEvent is { IsProcessed: false })
        {
            logger.LogDebug("Overriding scrobble event for {Series} from Read Status {Status} -> {UpdatedStatus}",
                series.Name, existingEvent.ReadStatus, status);

            existingEvent.ReadStatus = status;

            unitOfWork.ScrobbleRepository.Update(existingEvent);
            await unitOfWork.CommitAsync(ct);

            await auditService.LogScrobbleAsync(KavitaPlusEventType.ScrobbleEventUpdated, series.Id,
                new AuditLogScrobbleParamsDto
                {
                    Provider = Provider,
                    ScrobbleEventType = ScrobbleEventType.ReadStatusUpdate,
                    ReadStatus = status,
                }, AuditStatus.Info, userId: user.Id, ct: ct);
            return;
        }

        var scrobbleEvent = new ScrobbleEvent
        {
            ScrobbleEventType = ScrobbleEventType.ReadStatusUpdate,
            ScrobbleProvider = Provider,
            Format = series.Library.Type.ConvertToPlusMediaFormat(series.Format),
            SeriesId = series.Id,
            LibraryId = series.LibraryId,
            AppUserId = user.Id,
            ReadStatus = status,
        };

        SetScrobbleIds(scrobbleEvent, series);

        unitOfWork.ScrobbleRepository.Attach(scrobbleEvent);
        await unitOfWork.CommitAsync(ct);

        await auditService.LogScrobbleAsync(KavitaPlusEventType.ScrobbleEventCreated, series.Id,
            new AuditLogScrobbleParamsDto
            {
                Provider = Provider,
                ScrobbleEventType = ScrobbleEventType.ReadStatusUpdate,
                ReadStatus = status,
            }, AuditStatus.Info, userId: user.Id, ct: ct);

        logger.LogDebug("Created new scrobble event for {Series} with Read Status {Status}", series.Name, status);
    }

    public async Task ScrobbleRatingUpdate(AppUser user, Series series, Chapter? chapter, float rating, CancellationToken ct = default)
    {
        if (!SupportedEvents.Contains(ScrobbleEventType.ScoreUpdated) || chapter != null) return;

        var existingEvent = await unitOfWork.ScrobbleRepository.GetEvent(
            Provider, user.Id, series.Id, null, ScrobbleEventType.ScoreUpdated, true, ct
        );

        if (existingEvent is { IsProcessed: false })
        {
            logger.LogDebug("Overriding scrobble event for {Series} from Rating {Rating} -> {UpdatedRating}",
                series.Name, existingEvent.Rating, rating);

            existingEvent.Rating = rating;

            unitOfWork.ScrobbleRepository.Update(existingEvent);
            await unitOfWork.CommitAsync(ct);

            await auditService.LogScrobbleAsync(KavitaPlusEventType.ScrobbleEventUpdated, series.Id,
                new AuditLogScrobbleParamsDto
                {
                    Provider = Provider,
                    ScrobbleEventType = ScrobbleEventType.ScoreUpdated,
                    Rating = rating,
                    LibraryType = series.Library.Type,
                }, AuditStatus.Info, userId: user.Id, ct: ct);
            return;
        }

        var scrobbleEvent = new ScrobbleEvent
        {
            ScrobbleEventType = ScrobbleEventType.ScoreUpdated,
            ScrobbleProvider = Provider,
            Format = series.Library.Type.ConvertToPlusMediaFormat(series.Format),
            SeriesId = series.Id,
            LibraryId = series.LibraryId,
            AppUserId = user.Id,
            Rating = rating,
        };

        SetScrobbleIds(scrobbleEvent, series);

        unitOfWork.ScrobbleRepository.Attach(scrobbleEvent);
        await unitOfWork.CommitAsync(ct);

        await auditService.LogScrobbleAsync(KavitaPlusEventType.ScrobbleEventCreated, series.Id,
            new AuditLogScrobbleParamsDto
            {
                Provider = Provider,
                ScrobbleEventType = ScrobbleEventType.ScoreUpdated,
                Rating = rating,
                LibraryType = series.Library.Type,
            }, AuditStatus.Info, userId: user.Id, ct: ct);

        logger.LogDebug("Created new scrobble event for {Series} with Rating {Rating}", series.Name, rating);
    }

    public async Task ScrobbleReviewUpdate(AppUser user, Series series, Chapter? chapter, string? reviewTitle, string reviewBody,
        CancellationToken ct = default)
    {
        if (!SupportedEvents.Contains(ScrobbleEventType.Review) || chapter != null) return;

        var existingEvent = await unitOfWork.ScrobbleRepository.GetEvent(
            Provider, user.Id, series.Id, null, ScrobbleEventType.Review, true, ct
        );

        if (existingEvent is { IsProcessed: false })
        {
            logger.LogDebug("Overriding scrobble event for {Series} from Review Title {Title} -> {UpdatedTitle}",
                series.Name, existingEvent.ReviewTitle, reviewTitle);

            existingEvent.ReviewTitle = reviewTitle;
            existingEvent.ReviewBody = reviewBody;

            unitOfWork.ScrobbleRepository.Update(existingEvent);
            await unitOfWork.CommitAsync(ct);

            await auditService.LogScrobbleAsync(KavitaPlusEventType.ScrobbleEventUpdated, series.Id,
                new AuditLogScrobbleParamsDto
                {
                    Provider = Provider,
                    ScrobbleEventType = ScrobbleEventType.Review,
                    ReviewBody = reviewBody,
                    LibraryType = series.Library.Type,
                }, AuditStatus.Info, userId: user.Id, ct: ct);

            return;
        }

        var scrobbleEvent = new ScrobbleEvent
        {
            ScrobbleEventType = ScrobbleEventType.Review,
            ScrobbleProvider = Provider,
            Format = series.Library.Type.ConvertToPlusMediaFormat(series.Format),
            SeriesId = series.Id,
            LibraryId = series.LibraryId,
            AppUserId = user.Id,
            ReviewTitle = reviewTitle,
            ReviewBody = reviewBody,
        };

        SetScrobbleIds(scrobbleEvent, series);

        unitOfWork.ScrobbleRepository.Attach(scrobbleEvent);
        await unitOfWork.CommitAsync(ct);

        await auditService.LogScrobbleAsync(KavitaPlusEventType.ScrobbleEventCreated, series.Id,
            new AuditLogScrobbleParamsDto
            {
                Provider = Provider,
                ScrobbleEventType = ScrobbleEventType.Review,
                ReviewBody = reviewBody,
                LibraryType = series.Library.Type,
            }, AuditStatus.Info, userId: user.Id, ct: ct);

        logger.LogDebug("Created new review scrobble event for {Series}", series.Name);
    }

    public async Task ScrobbleReadingUpdate(AppUser user, Series series, Chapter chapter, CancellationToken ct = default)
    {
        if (!SupportedEvents.Contains(ScrobbleEventType.ChapterRead)) return;

        // Series should only create scrobble events for completed chapters
        var chapterProgress = await unitOfWork.AppUserProgressRepository.GetUserProgressAsync(chapter.Id, user.Id, ct);
        if (chapterProgress?.PagesRead != 0 && chapterProgress?.PagesRead < chapter.Pages) return;

        var isAnyProgressOnSeries = await unitOfWork.AppUserProgressRepository.HasAnyProgressOnSeriesAsync(series.Id, user.Id, ct);

        var volumeNumber = (int) await unitOfWork.AppUserProgressRepository.GetHighestFullyReadVolumeForSeries(series.Id, user.Id, ct);
        var chapterNumber = await unitOfWork.AppUserProgressRepository.GetHighestFullyReadChapterForSeries(series.Id, user.Id, ct);

        var existingEvent = await unitOfWork.ScrobbleRepository.GetEvent(
            Provider, user.Id, series.Id, null, ScrobbleEventType.ChapterRead, true, ct
        );

        if (existingEvent is { IsProcessed: false })
        {
            if (!isAnyProgressOnSeries)
            {
                logger.LogDebug("Removing scrobble event for {Series} as there is no reading progress", series.Name);

                unitOfWork.ScrobbleRepository.Remove(existingEvent);

                await unitOfWork.CommitAsync(ct);
                return;
            }

            var prevChapterNumber = existingEvent.ChapterNumber;
            var prevVolumeNumber = existingEvent.VolumeNumber;

            existingEvent.VolumeNumber = volumeNumber;
            existingEvent.ChapterNumber = chapterNumber;

            unitOfWork.ScrobbleRepository.Update(existingEvent);
            await unitOfWork.CommitAsync(ct);

            await auditService.LogScrobbleAsync(KavitaPlusEventType.ScrobbleEventUpdated, series.Id,
                new AuditLogScrobbleParamsDto
                {
                    Provider = Provider,
                    ScrobbleEventType = ScrobbleEventType.ChapterRead,
                    ChapterNumber = chapterNumber,
                    VolumeNumber = volumeNumber,
                    LibraryType = series.Library.Type,
                }, AuditStatus.Info, userId: user.Id, ct: ct);

            logger.LogDebug("Updated scrobble event for {Series} from Volume {PrevVolume} -> {Volume}, Chapter {PrevChapter} -> {Chapter}",
                series.Name, prevVolumeNumber, volumeNumber, prevChapterNumber, chapterNumber);
            return;
        }

        // Do not create a new scrobble event if there is no progress
        if (!isAnyProgressOnSeries) return;

        var evt = new ScrobbleEvent
        {
            SeriesId = series.Id,
            LibraryId = series.LibraryId,
            ScrobbleEventType = ScrobbleEventType.ChapterRead,
            ScrobbleProvider = Provider,
            AppUserId = user.Id,
            VolumeNumber = volumeNumber,
            ChapterNumber = chapterNumber,
            Format = series.Library.Type.ConvertToPlusMediaFormat(series.Format),
        };

        SetScrobbleIds(evt, series);

        if (evt.VolumeNumber is Parser.SpecialVolumeNumber)
        {
            // We don't process Specials because they will never match on AniList
            return;
        }

        unitOfWork.ScrobbleRepository.Attach(evt);
        await unitOfWork.CommitAsync(ct);

        await auditService.LogScrobbleAsync(KavitaPlusEventType.ScrobbleEventCreated, series.Id,
            new AuditLogScrobbleParamsDto
            {
                Provider = Provider,
                ScrobbleEventType = ScrobbleEventType.ChapterRead,
                ChapterNumber = chapterNumber,
                VolumeNumber = volumeNumber,
                LibraryType = series.Library.Type,
            }, AuditStatus.Info, userId: user.Id, ct: ct);

        logger.LogDebug("Created new scrobble read event for {Series} with Volume {Volume}, Chapter {Chapter} for User: {UserId}",
            series.Name, volumeNumber, chapterNumber, user.Id);

    }

    public async Task ScrobbleWantToReadUpdate(AppUser user, Series series, Chapter chapter, bool onWantToRead,
        CancellationToken ct = default)
    {
        if (!SupportedEvents.Contains(ScrobbleEventType.AddWantToRead) || !SupportedEvents.Contains(ScrobbleEventType.RemoveWantToRead)) return;

        var eventType = onWantToRead ? ScrobbleEventType.AddWantToRead : ScrobbleEventType.RemoveWantToRead;

        var existingEvents = (await unitOfWork.ScrobbleRepository.GetUserEventsForSeries(user.Id, series.Id, ct))
            .Where(e => e.ScrobbleProvider == Provider)
            .Where(e => e.ScrobbleEventType is ScrobbleEventType.AddWantToRead or ScrobbleEventType.RemoveWantToRead);

        unitOfWork.ScrobbleRepository.Remove(existingEvents);

        var scrobbleEvent = new ScrobbleEvent
        {
            ScrobbleProvider = Provider,
            AppUserId = user.Id,
            LibraryId = series.LibraryId,
            SeriesId = series.Id,
            Format = series.Library.Type.ConvertToPlusMediaFormat(series.Format),
            ScrobbleEventType = eventType,
        };

        SetScrobbleIds(scrobbleEvent, series);

        unitOfWork.ScrobbleRepository.Attach(scrobbleEvent);
        await unitOfWork.CommitAsync(ct);

        await auditService.LogScrobbleAsync(KavitaPlusEventType.ScrobbleEventCreated, series.Id,
            new AuditLogScrobbleParamsDto
            {
                Provider = Provider,
                ScrobbleEventType = eventType,
                LibraryType = series.Library.Type,
            }, AuditStatus.Info, userId: user.Id, ct: ct);

        logger.LogDebug("Created new scrobble {Event} event for {Series} for User: {UserId}",
            eventType, series.Name, user.Id);
    }
}
