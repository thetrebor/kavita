using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kavita.API.Database;
using Kavita.API.Services.Plus;
using Kavita.Models.DTOs.Scrobbling;
using Kavita.Models.Entities;
using Kavita.Models.Entities.Enums;
using Kavita.Models.Entities.Scrobble;
using Kavita.Models.Entities.User;
using Kavita.Models.Extensions;
using Microsoft.Extensions.Logging;

namespace Kavita.Services.Plus.ScrobbleService;

/// <summary>
/// A <see cref="IScrobbleProviderService"/> implementation for <see cref="ScrobbleProvider"/>'s that track data
/// based on chapters. (Hardcover)
/// </summary>
public abstract class ChapterScrobbleService<T>(ILogger<T> logger, IUnitOfWork unitOfWork): IScrobbleProviderService
where T: IScrobbleProviderService
{
    protected abstract ScrobbleProvider Provider { get; }

    protected abstract IReadOnlyList<ScrobbleEventType> SupportedEvents { get; }

    protected abstract void SetScrobbleIds(ScrobbleEvent evt, Series series, Chapter chapter);

    public async Task ScrobbleRatingUpdate(AppUser user, Series series, Chapter? chapter, float rating, CancellationToken ct = default)
    {
        if (!SupportedEvents.Contains(ScrobbleEventType.ScoreUpdated) || chapter == null) return;

        var existingEvent = await unitOfWork.ScrobbleRepository.GetEvent(
            Provider, user.Id, series.Id, chapter.Id, ScrobbleEventType.ScoreUpdated, true, ct
            );

        if (existingEvent is { IsProcessed: false })
        {
            logger.LogDebug("[{Provider}] Overriding scrobble event for {Series}, ChapterId: {ChapterId} from Rating {Rating} -> {UpdatedRating}",
                Provider, series.Name, chapter.Id, existingEvent.Rating, rating);

            existingEvent.Rating = rating;

            unitOfWork.ScrobbleRepository.Update(existingEvent);
            await unitOfWork.CommitAsync(ct);
            return;
        }

        var scrobbleEvent = new ScrobbleEvent
        {
            ScrobbleEventType = ScrobbleEventType.ScoreUpdated,
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

        logger.LogDebug("[{Provider}] Created new scrobble event for {Series}, ChapterId: {ChapterId} with Rating {Rating}",
            Provider, series.Name, chapter.Id, rating);
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
            logger.LogDebug("[{Provider}] Overriding scrobble event for {Series} - ChapterId: {ChapterId} from Review Title {Title} -> {UpdatedTitle}",
                Provider, series.Name, chapter.Id, existingEvent.ReviewTitle, reviewTitle);

            existingEvent.ReviewTitle = reviewTitle;
            existingEvent.ReviewBody = reviewBody;

            unitOfWork.ScrobbleRepository.Update(existingEvent);
            await unitOfWork.CommitAsync(ct);
            return;
        }

        var scrobbleEvent = new ScrobbleEvent
        {
            ScrobbleEventType = ScrobbleEventType.Review,
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

        logger.LogDebug("[{Provider}] Created new scrobble event for {Series} - ChapterId: {ChapterId} with Review Title {Title}",
            Provider, series.Name, chapter.Id, reviewTitle);

    }

    public async Task ScrobbleReadingUpdate(AppUser user, Series series, Chapter chapter, CancellationToken ct = default)
    {
        if (!SupportedEvents.Contains(ScrobbleEventType.ChapterRead)) return;

        var chapterProgress = await unitOfWork.AppUserProgressRepository.GetUserProgressAsync(chapter.Id, user.Id, ct);
        var hasAnyProgress = chapterProgress is not { PagesRead: > 0 };
        var existingEvent = await unitOfWork.ScrobbleRepository.GetEvent(
            Provider, user.Id, series.Id, chapter.Id, ScrobbleEventType.ChapterRead, true, ct
        );

        var currentProgress = chapterProgress != null ? chapterProgress.PagesRead / (float)chapter.Pages : 0f;

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

            logger.LogDebug("[{Provider}] Overriding scrobble event for {Series} - ChapterId: {ChapterId} from {PrevProgress}% -> {Progress}%",
                Provider, series.Name, chapter.Id, prevProgress, currentProgress);
            return;
        }

        if (!hasAnyProgress) return;

        var evt = new ScrobbleEvent
        {
            ScrobbleEventType = ScrobbleEventType.ChapterRead,
            Format = series.Library.Type.ConvertToPlusMediaFormat(series.Format),
            SeriesId = series.Id,
            ChapterId = chapter.Id,
            LibraryId = series.LibraryId,
            AppUserId = user.Id,
            Progress = currentProgress,
        };

        SetScrobbleIds(evt, series, chapter);

        unitOfWork.ScrobbleRepository.Attach(evt);
        await unitOfWork.CommitAsync(ct);

        logger.LogDebug("[{Provider}] Created new scrobble event for {Series} - ChapterId: {ChapterId} with {Progress}%",
            Provider, series.Name, chapter.Id, currentProgress);
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

        logger.LogDebug("[{Provider}] Created new scrobble {EventType} event for {Series} - ChapterId: {ChapterId}",
            Provider, eventType, series.Name, chapter.Id);

    }
}
