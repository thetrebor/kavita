using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using API.Data;
using API.Entities.Progress;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace API.Services;
#nullable enable

public interface IDeviceTrackingService
{
    Task<int> TrackDeviceAsync(int userId, ClientInfoData clientInfo, string? uiFingerprint, CancellationToken ct);
    Task ClearDeviceCacheAsync(int deviceId);
    Task ClearUserDeviceCachesAsync(int userId);
}

public class DeviceTrackingService(HybridCache cache, DataContext context, ILogger<DeviceTrackingService> logger, IClientDeviceService clientDeviceService) : IDeviceTrackingService
{

    private static readonly HybridCacheEntryOptions CacheOptions = new()
    {
        Expiration = TimeSpan.FromMinutes(10),
        LocalCacheExpiration = TimeSpan.FromMinutes(10)
    };


    public async Task<int> TrackDeviceAsync(int userId, ClientInfoData clientInfo, string? uiFingerprint, CancellationToken ct)
    {
        var deviceIdPart = string.IsNullOrEmpty(uiFingerprint) ? "unknown" : uiFingerprint;
        var cacheKey = $"device_tracking_{userId}_{deviceIdPart}";

        var deviceId = await cache.GetOrCreateAsync(
            cacheKey,
            (userId, clientInfo, uiFingerprint, clientDeviceService),
            async (state, cancel) =>
            {
                var device = await state.clientDeviceService.IdentifyOrRegisterDeviceAsync(
                    state.userId,
                    state.clientInfo,
                    state.uiFingerprint,
                    cancel);
                return device.Id;
            },
            CacheOptions,
            cancellationToken: ct);

        // Store reverse mapping: deviceId (int) -> cacheKey (string)
        var mappingKey = $"device_key_mapping_{deviceId}";
        await cache.SetAsync(mappingKey, cacheKey, CacheOptions, cancellationToken: ct);

        return deviceId;
    }

    public async Task ClearDeviceCacheAsync(int deviceId)
    {
        var mappingKey = $"device_key_mapping_{deviceId}";

        try
        {
            var cacheKey = await cache.GetOrCreateAsync<string?>(
                mappingKey,
                (ct) => ValueTask.FromResult<string?>(null),
                cancellationToken: CancellationToken.None);

            if (!string.IsNullOrEmpty(cacheKey))
            {
                await cache.RemoveAsync(cacheKey);
            }

            await cache.RemoveAsync(mappingKey);

            logger.LogDebug("Cleared device cache for device {DeviceId}", deviceId);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to clear device cache for device {DeviceId}", deviceId);
        }
    }

    public async Task ClearUserDeviceCachesAsync(int userId)
    {
        var allActivityData = await context.AppUserReadingSession
            .Where(s => s.AppUserId == userId)
            .Select(s => s.ActivityData)
            .ToListAsync();
        var allDeviceIds = allActivityData
            .SelectMany(s => s.Select(s2 => s2.DeviceId))
            .Distinct()
            .ToList();

        foreach (var deviceId in allDeviceIds)
        {
            // TODO: Optimize this code
            await ClearDeviceCacheAsync(deviceId);
        }
        logger.LogWarning("ClearUserDeviceCachesAsync not fully implemented - requires DB query");
    }
}
