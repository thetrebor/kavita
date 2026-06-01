using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kavita.API.Database;
using Kavita.API.Services.Plus;
using Kavita.Models.DTOs.KavitaPlus;
using Kavita.Models.DTOs.KavitaPlus.Scrobble;
using Kavita.Models.DTOs.Scrobbling;
using Kavita.Models.Entities;
using Kavita.Models.Entities.Enums;
using Kavita.Models.Entities.Enums.Audit;
using Kavita.Models.Entities.Enums.UserPreferences;
using Kavita.Models.Entities.Scrobble;
using Kavita.Models.Entities.User;
using Kavita.Models.Extensions;
using Microsoft.Extensions.Logging;

namespace Kavita.Services.Plus.ScrobbleService;

/// <summary>
/// A <see cref="IScrobbleProviderService"/> implementation for <see cref="ScrobbleProvider"/>'s that track data
/// based on chapters. (Hardcover)
/// </summary>
public abstract class ChapterScrobbleService<T>(ILogger<T> logger, IUnitOfWork unitOfWork, IKavitaPlusAuditService auditService): IScrobbleProviderService
where T: IScrobbleProviderService
{
    protected abstract ScrobbleProvider Provider { get; }

    protected abstract IReadOnlyList<ScrobbleEventType> SupportedEvents { get; }

    protected abstract void SetScrobbleIds(ScrobbleEvent evt, Series series, Chapter chapter);

    public abstract bool IsTokenValid(string token);

    public async Task ScrobbleReadStatusUpdates(AppUser user, Series series, Chapter? chapter, ScrobbleReadStatus status,
        CancellationToken ct = default)
    {
        if (!SupportedEvents.Contains(ScrobbleEventType.ReadStatusUpdate) || chapter == null) return;

        var existingEvent = await unitOfWork.ScrobbleRepository.GetEvent(
            Provider, user.Id, series.Id, chapter.Id, ScrobbleEventType.ReadStatusUpdate, true, ct
        );

        if (existingEvent is { IsProcessed: false })
        {
            logger.LogDebug("Overriding scrobble event for {Series} - {ChapterId} from Read Status {Status} -> {UpdatedStatus}",
                series.Name, chapter.Id, existingEvent.ReadStatus, status);

            existingEvent.ReadStatus = status;

            unitOfWork.ScrobbleRepository.Update(existingEvent);
            await unitOfWork.CommitAsync(ct);

            await auditService.LogChapterScrobbleAsync(KavitaPlusEventType.ScrobbleEventUpdated, series.Id, chapter.Id,
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
            ChapterId = chapter.Id,
            LibraryId = series.LibraryId,
            AppUserId = user.Id,
            ReadStatus = status,
        };

        SetScrobbleIds(scrobbleEvent, series, chapter);

        unitOfWork.ScrobbleRepository.Attach(scrobbleEvent);
        await unitOfWork.CommitAsync(ct);

        await auditService.LogChapterScrobbleAsync(KavitaPlusEventType.ScrobbleEventCreated, series.Id, chapter.Id,
            new AuditLogScrobbleParamsDto
            {
                Provider = Provider,
                ScrobbleEventType = ScrobbleEventType.ReadStatusUpdate,
                ReadStatus = status,
            }, AuditStatus.Info, userId: user.Id, ct: ct);

        logger.LogDebug("Created new scrobble event for {Series} - {ChapterId} with Read Status {Status}", series.Name, chapter.Id, status);
    }

    public async Task ScrobbleRatingUpdate(AppUser user, Series series, Chapter? chapter, float rating, CancellationToken ct = default)
    {
        if (!SupportedEvents.Contains(ScrobbleEventType.ScoreUpdated) || chapter == null) return;

        var existingEvent = await unitOfWork.ScrobbleRepository.GetEvent(
            Provider, user.Id, series.Id, chapter.Id, ScrobbleEventType.ScoreUpdated, true, ct
            );

        if (existingEvent is { IsProcessed: false })
        {
            logger.LogDebug("Overriding scrobble event for {Series}, ChapterId: {ChapterId} from Rating {Rating} -> {UpdatedRating}",
                series.Name, chapter.Id, existingEvent.Rating, rating);

            existingEvent.Rating = rating;

            unitOfWork.ScrobbleRepository.Update(existingEvent);
            await unitOfWork.CommitAsync(ct);

            await auditService.LogChapterScrobbleAsync(KavitaPlusEventType.ScrobbleEventUpdated, series.Id, chapter.Id,
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
            ChapterId = chapter.Id,
            LibraryId = series.LibraryId,
            AppUserId = user.Id,
            Rating = rating,
        };

        SetScrobbleIds(scrobbleEvent, series, chapter);

        unitOfWork.ScrobbleRepository.Attach(scrobbleEvent);
        await unitOfWork.CommitAsync(ct);

        await auditService.LogChapterScrobbleAsync(KavitaPlusEventType.ScrobbleEventCreated, series.Id, chapter.Id,
            new AuditLogScrobbleParamsDto
            {
                Provider = Provider,
                ScrobbleEventType = ScrobbleEventType.ScoreUpdated,
                Rating = rating,
                LibraryType = series.Library.Type,
            }, AuditStatus.Info, userId: user.Id, ct: ct);

        logger.LogDebug("Created new scrobble event for {Series}, ChapterId: {ChapterId} with Rating {Rating}",
            series.Name, chapter.Id, rating);
    }

    public async Task ScrobbleReviewUpdate(AppUser user, Series series, Chapter? chapter, string? reviewTitle, string reviewBody,
        CancellationToken ct = default)
    {
        if (!SupportedEvents.Contains(ScrobbleEventType.Review) || chapter == null) return;

        var existingEvent = await unitOfWork.ScrobbleRepository.GetEvent(
            Provider, user.Id, series.Id, chapter.Id, ScrobbleEventType.Review, true, ct
        );

        if (existingEvent is { IsProcessed: false })
        {
            logger.LogDebug("Overriding scrobble event for {Series} - ChapterId: {ChapterId} from Review Title {Title} -> {UpdatedTitle}",
                series.Name, chapter.Id, existingEvent.ReviewTitle, reviewTitle);

            existingEvent.ReviewTitle = reviewTitle;
            existingEvent.ReviewBody = reviewBody;

            unitOfWork.ScrobbleRepository.Update(existingEvent);
            await unitOfWork.CommitAsync(ct);

            await auditService.LogChapterScrobbleAsync(KavitaPlusEventType.ScrobbleEventUpdated, series.Id, chapter.Id,
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
            ChapterId = chapter.Id,
            LibraryId = series.LibraryId,
            AppUserId = user.Id,
            ReviewTitle = reviewTitle,
            ReviewBody = reviewBody,
        };

        SetScrobbleIds(scrobbleEvent, series, chapter);

        unitOfWork.ScrobbleRepository.Attach(scrobbleEvent);
        await unitOfWork.CommitAsync(ct);

        await auditService.LogChapterScrobbleAsync(KavitaPlusEventType.ScrobbleEventCreated, series.Id, chapter.Id,
            new AuditLogScrobbleParamsDto
            {
                Provider = Provider,
                ScrobbleEventType = ScrobbleEventType.Review,
                ReviewBody = reviewBody,
                LibraryType = series.Library.Type,
            }, AuditStatus.Info, userId: user.Id, ct: ct);

        logger.LogDebug("Created new scrobble event for {Series} - ChapterId: {ChapterId} with Review Title {Title}",
            series.Name, chapter.Id, reviewTitle);

    }

    public async Task ScrobbleReadingUpdate(AppUser user, Series series, Chapter chapter, CancellationToken ct = default)
    {
        if (!SupportedEvents.Contains(ScrobbleEventType.ChapterRead)) return;

        var chapterProgress = await unitOfWork.AppUserProgressRepository.GetUserProgressAsync(chapter.Id, user.Id, ct);
        var hasAnyProgress = chapterProgress is { PagesRead: > 0 };
        var existingEvent = await unitOfWork.ScrobbleRepository.GetEvent(
            Provider, user.Id, series.Id, chapter.Id, ScrobbleEventType.ChapterRead, true, ct
        );

        var currentProgress = (float) Math.Ceiling((chapterProgress != null ? chapterProgress.PagesRead / (float)chapter.Pages : 0f) * 100);

        if (existingEvent is { IsProcessed: false })
        {
            if (!hasAnyProgress)
            {
                logger.LogDebug("[{Provider}] Removing scrobble event for {Series} - ChapterId: {ChapterId} as there is no reading progress",
                    Provider, series.Name, chapter.Id);

                unitOfWork.ScrobbleRepository.Remove(existingEvent);

                await unitOfWork.CommitAsync(ct);
                return;
            }

            var prevProgress = existingEvent.Progress;
            existingEvent.Progress = currentProgress;

            unitOfWork.ScrobbleRepository.Update(existingEvent);
            await unitOfWork.CommitAsync(ct);

            // Note that this will generate a lot of events if the book has very short pages
            // Hardcover is only enabled for Light Novel & Book libraries, so I think this should be alright
            // and is worth it having a correct view of the events
            await auditService.LogChapterScrobbleAsync(KavitaPlusEventType.ScrobbleEventUpdated, series.Id, chapter.Id,
                new AuditLogScrobbleParamsDto
                {
                    Provider = Provider,
                    ScrobbleEventType = ScrobbleEventType.ChapterRead,
                    PercentRead = currentProgress,
                    LibraryType = series.Library.Type,
                }, AuditStatus.Info, userId: user.Id, ct: ct);

            logger.LogDebug("Overriding scrobble event for {Series} - ChapterId: {ChapterId} from {PrevProgress}% -> {Progress}%",
                series.Name, chapter.Id, prevProgress, currentProgress);
            return;
        }

        if (!hasAnyProgress) return;

        var evt = new ScrobbleEvent
        {
            ScrobbleEventType = ScrobbleEventType.ChapterRead,
            ScrobbleProvider = Provider,
            Format = series.Library.Type.ConvertToPlusMediaFormat(series.Format),
            SeriesId = series.Id,
            ChapterId = chapter.Id,
            LibraryId = series.LibraryId,
            ChapterNumber = (int) chapter.MaxNumber,
            VolumeNumber = chapter.Volume?.MaxNumber,
            AppUserId = user.Id,
            Progress = currentProgress,
        };

        SetScrobbleIds(evt, series, chapter);

        unitOfWork.ScrobbleRepository.Attach(evt);
        await unitOfWork.CommitAsync(ct);

        await auditService.LogChapterScrobbleAsync(KavitaPlusEventType.ScrobbleEventCreated, series.Id, chapter.Id,
            new AuditLogScrobbleParamsDto
            {
                Provider = Provider,
                ScrobbleEventType = ScrobbleEventType.ChapterRead,
                ChapterNumber = (int) chapter.MaxNumber,
                VolumeNumber = chapter.Volume?.MaxNumber,
                PercentRead = currentProgress,
                LibraryType = series.Library.Type,
            }, AuditStatus.Info, userId: user.Id, ct: ct);

        logger.LogDebug("Created new scrobble event for {Series} - ChapterId: {ChapterId} with {Progress}%",
            series.Name, chapter.Id, currentProgress);
    }

    public async Task ScrobbleWantToReadUpdate(AppUser user, Series series, Chapter chapter, bool onWantToRead,
        CancellationToken ct = default)
    {
        if (!SupportedEvents.Contains(ScrobbleEventType.AddWantToRead) || !SupportedEvents.Contains(ScrobbleEventType.RemoveWantToRead)) return;

        var eventType = onWantToRead ? ScrobbleEventType.AddWantToRead : ScrobbleEventType.RemoveWantToRead;

        var existingEvents = (await unitOfWork.ScrobbleRepository.GetUserEventsForSeries(user.Id, series.Id, ct))
            .Where(e => e.ScrobbleProvider == Provider && e.ChapterId == chapter.Id)
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
            ChapterId = chapter.Id,
        };

        SetScrobbleIds(scrobbleEvent, series, chapter);

        unitOfWork.ScrobbleRepository.Attach(scrobbleEvent);
        await unitOfWork.CommitAsync(ct);

        await auditService.LogChapterScrobbleAsync(KavitaPlusEventType.ScrobbleEventUpdated, series.Id, chapter.Id,
            new AuditLogScrobbleParamsDto
            {
                Provider = Provider,
                ScrobbleEventType = eventType,
                LibraryType = series.Library.Type,
            }, AuditStatus.Info, userId: user.Id, ct: ct);

        logger.LogDebug("Created new scrobble {EventType} event for {Series} - ChapterId: {ChapterId}",
            eventType, series.Name, chapter.Id);

    }
}
