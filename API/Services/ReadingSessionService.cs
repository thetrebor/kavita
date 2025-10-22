using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using API.Data;
using API.DTOs.Progress;
using API.Entities.Enums;
using API.Entities.Progress;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace API.Services;

public interface IReadingSessionService
{
    Task UpdateProgress(int userId, ProgressDto progressDto);
}

internal sealed record SessionTimeout<T>
{
    public T Value { get; set; }
    /// <summary>
    /// Expiration time in Utc
    /// </summary>
    public DateTime Expiration { get; set; }
    public DateTime LastTimerRefresh { get; set; }
    public Timer TimeoutTimer { get; set; }
}

public class ReadingSessionService : IReadingSessionService, IDisposable, IAsyncDisposable
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<ReadingSessionService> _logger;
    private readonly ConcurrentDictionary<string, SessionTimeout<int>> _activeSessions = new();
    private readonly int _defaultTimeoutMinutes;
    private readonly int _timerRefreshDebounceSeconds;
    private Timer _midnightRolloverTimer;
    private bool _disposed;

    public ReadingSessionService(IServiceScopeFactory serviceScopeFactory, ILogger<ReadingSessionService> logger,
        int defaultTimeoutMinutes = 400, int timerRefreshDebounceSeconds = 5)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;

        _defaultTimeoutMinutes = defaultTimeoutMinutes;
        _timerRefreshDebounceSeconds = timerRefreshDebounceSeconds;

        ScheduleMidnightRollover();
    }


    public async Task UpdateProgress(int userId, ProgressDto progressDto)
    {
        _logger.LogDebug("Creating/Updating Reading Session for {UserId} on {ChapterId}", userId, progressDto.ChapterId);
        var session = await GetOrCreateSession(userId, progressDto);

        // Update session activity data in DB
        using var scope = _serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DataContext>();

        // If Chapter doesn't exist already, add
        var existingChapterActivity = session.ActivityData.FirstOrDefault(d => d.ChapterId == progressDto.ChapterId);

        if (existingChapterActivity != null)
        {
            existingChapterActivity.PagesRead = progressDto.PageNum - existingChapterActivity.StartPage;
            existingChapterActivity.EndPage = progressDto.PageNum;
            existingChapterActivity.EndTime = DateTime.Now;
            existingChapterActivity.EndTimeUtc = DateTime.UtcNow;

            var chapterFormat = await context.MangaFile
                .Where(f => f.ChapterId == progressDto.ChapterId)
                .Select(f => f.Format)
                .FirstOrDefaultAsync();

            if (chapterFormat == MangaFormat.Epub && !string.IsNullOrEmpty(progressDto.BookScrollId))
            {
                var bookService = scope.ServiceProvider.GetRequiredService<IBookService>();
                var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();

                var chapter = await cacheService.Ensure(progressDto.ChapterId);
                var cachedFilePath = cacheService.GetCachedFile(chapter!);

                // First update - capture starting position
                if (string.IsNullOrEmpty(existingChapterActivity.StartBookScrollId))
                {
                    existingChapterActivity.StartBookScrollId = progressDto.BookScrollId;
                    existingChapterActivity.WordsRead = 0;
                }
                else
                {
                    // Calculate total words read from start to current position
                    try
                    {
                        existingChapterActivity.WordsRead = await bookService.GetWordCountBetweenXPaths(
                            cachedFilePath,
                            existingChapterActivity.StartBookScrollId,
                            progressDto.BookScrollId
                        );
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "There was an error calculating words read for reading session {SessionId} on book {File}", session.Id, cachedFilePath);
                    }
                }

                // Always update the current end position
                existingChapterActivity.EndBookScrollId = progressDto.BookScrollId;
            }
        }
        else
        {
            // Add new ActivityData for a different chapter in the same session
            session.ActivityData.Add(NewActivityData(progressDto));
        }

        // Update session timestamps
        session.LastModified = DateTime.Now;
        session.LastModifiedUtc = DateTime.UtcNow;

        // Save changes
        context.AppUserReadingSession.Update(session);
        await context.SaveChangesAsync();


        // Refresh timeout
        var cacheKey = GenerateCacheKey(userId, progressDto.ChapterId);
        RefreshSessionTimeout(cacheKey, session.Id);

    }

    private async Task<AppUserReadingSession> GetOrCreateSession(int userId, ProgressDto dto)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DataContext>();

        // Check if we have an existing cached reading session that is active
        var cacheKey = GenerateCacheKey(userId, dto.ChapterId);
        if (_activeSessions.TryGetValue(cacheKey, out var sessionTimeout))
        {
            if (sessionTimeout.Expiration <= DateTime.Now)
            {
                // Expired - close it and create new one
                await CloseSession(cacheKey, sessionTimeout.Value);
            }
            else
            {
                return await context.AppUserReadingSession
                    .Where(s => s.Id == sessionTimeout.Value)
                    .FirstOrDefaultAsync();
            }
        }

        // Look up in the DB for an active reading session
        var dbSession = await context.AppUserReadingSession
            .Where(s => s.IsActive && s.AppUserId == userId)
            .FirstOrDefaultAsync();

        if (dbSession != null)
        {
            // Re-add to cache with timer
            RefreshSessionTimeout(cacheKey, dbSession.Id);
            return dbSession;
        }

        // Create a new session and return it
        var newSession = new AppUserReadingSession()
            {
                AppUserId = userId,
                StartTime = DateTime.Now,
                StartTimeUtc = DateTime.UtcNow,
                IsActive = true,
                ActivityData =
                [
                    NewActivityData(dto)
                ]
            };

        await context.AppUserReadingSession.AddAsync(newSession);
        await context.SaveChangesAsync();

        RefreshSessionTimeout(cacheKey, newSession.Id);

        return newSession;
    }

    private static ReadingActivityDataDto NewActivityData(ProgressDto dto)
    {
        return new ReadingActivityDataDto
        {
            ChapterId = dto.ChapterId,
            VolumeId = dto.VolumeId,
            SeriesId = dto.SeriesId,
            LibraryId = dto.LibraryId,
            StartPage = dto.PageNum,
            EndPage = dto.PageNum,
            StartTime = DateTime.Now,
            StartTimeUtc = DateTime.UtcNow,
            EndTime = null,
            PagesRead = 0,
            WordsRead = 0,
        };
    }


    private void RefreshSessionTimeout(string cacheKey, int sessionId)
    {
        var now = DateTime.Now;

        _activeSessions.AddOrUpdate(cacheKey,
            // Add new
            key => new SessionTimeout<int>()
            {
                Value = sessionId,
                Expiration = now.AddMinutes(_defaultTimeoutMinutes),
                LastTimerRefresh = now,
                TimeoutTimer = CreateSessionTimer(key, sessionId)
            },
            // Update Existing
            (_, existing) =>
            {
                // Always update expiration
                existing.Expiration = now.AddMinutes(_defaultTimeoutMinutes);

                // Debounce timer refresh (avoid excessive timer churn)
                var secondsSinceLastRefresh = (now - existing.LastTimerRefresh).TotalSeconds;
                if (secondsSinceLastRefresh >= _timerRefreshDebounceSeconds)
                {
                    existing.TimeoutTimer?.Change(TimeSpan.FromMinutes(_defaultTimeoutMinutes), TimeSpan.Zero);

                    existing.LastTimerRefresh = now;
                }

                return existing;
            }
        );
    }

    private Timer CreateSessionTimer(string cacheKey, int sessionId)
    {
        _logger.LogDebug("Creating timer for session {SessionId} with key {CacheKey}, will fire in {Minutes} minutes",
            sessionId, cacheKey, _defaultTimeoutMinutes);
        return new Timer(
            callback: _ => OnSessionTimeout(cacheKey, sessionId),
            state: null,
            dueTime: TimeSpan.FromMinutes(_defaultTimeoutMinutes),
            period: TimeSpan.Zero
        );
    }

    private void OnSessionTimeout(string cacheKey, int sessionId)
    {
        _ = Task.Run(async () => await CloseSession(cacheKey, sessionId))
            .ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    _logger.LogError(t.Exception, "There was an issue closing session {SessionId} with CacheKey: {CacheKey}",
                        sessionId, cacheKey);
                }
            });
    }

    private async Task CloseSession(string cacheKey, int sessionId)
    {
        // Remove from cache and dispose timer
        if (_activeSessions.TryRemove(cacheKey, out var session) && session.TimeoutTimer != null)
        {
            await session.TimeoutTimer.DisposeAsync();
        }

        using var scope = _serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DataContext>();

        // Mark session as inactive in DB
        await context.AppUserReadingSession
            .Where(s => s.Id == sessionId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.IsActive, false)
                .SetProperty(x => x.EndTime, DateTime.Now)
                .SetProperty(x => x.EndTimeUtc, DateTime.UtcNow)
                .SetProperty(x => x.LastModified, DateTime.Now)
                .SetProperty(x => x.LastModifiedUtc, DateTime.UtcNow));
    }

    private void ScheduleMidnightRollover()
    {
        var now = DateTime.Now;
        var nextMidnight = now.Date.AddDays(1);
        var timeUntilMidnight = nextMidnight - now;

        _midnightRolloverTimer = new Timer(
            callback: _ =>
            {
                // Synchronous callback that starts async work
                OnMidnightRolloverAsync().ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        _logger.LogCritical("There was an issue closing midnight sessions");
                    }
                });
            },
            state: null,
            dueTime: timeUntilMidnight,
            period: TimeSpan.Zero
        );
    }

    private async Task OnMidnightRolloverAsync()
    {
        var endOfYesterday = DateTime.Now.Date.AddTicks(-1); // 23:59:59.9999999
        var endOfYesterdayUtc = DateTime.UtcNow.Date.AddTicks(-1); // 23:59:59.9999999
        var sessionsToClose = _activeSessions.ToArray();

        if (sessionsToClose.Length > 0)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DataContext>();

            var sessionIds = sessionsToClose.Select(kvp => kvp.Value.Value).ToList();

            // Batch close all sessions in DB
            await context.AppUserReadingSession
                .Where(s => sessionIds.Contains(s.Id))
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.IsActive, false)
                    .SetProperty(x => x.EndTime, endOfYesterday)
                    .SetProperty(x => x.EndTimeUtc, endOfYesterdayUtc)
                    .SetProperty(x => x.LastModified, DateTime.Now)
                    .SetProperty(x => x.LastModifiedUtc, DateTime.UtcNow));

            // Clear cache and dispose all timers
            foreach (var kvp in sessionsToClose)
            {
                if (kvp.Value.TimeoutTimer != null) await kvp.Value.TimeoutTimer.DisposeAsync();
                _activeSessions.TryRemove(kvp.Key, out _);
            }
        }

        // Schedule next midnight Rollover
        ScheduleMidnightRollover();
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);

        Dispose(disposing: false);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            foreach (var session in _activeSessions.Values)
            {
                session.TimeoutTimer?.Dispose();
            }

            _midnightRolloverTimer?.Dispose();
            _activeSessions.Clear();
        }

        _disposed = true;
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (_disposed) return;

        // Dispose managed resources asynchronously
        foreach (var session in _activeSessions.Values)
        {
            if (session.TimeoutTimer != null)
            {
                await session.TimeoutTimer.DisposeAsync().ConfigureAwait(false);
            }
        }

        if (_midnightRolloverTimer != null)
        {
            await _midnightRolloverTimer.DisposeAsync().ConfigureAwait(false);
        }

        _activeSessions.Clear();

        _disposed = true;
    }

    private static string GenerateCacheKey(int userId, int chapterId)
    {
        return $"{userId}_{chapterId}";
    }
}
